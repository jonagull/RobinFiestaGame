extends Node2D

const DEATH_CTR_GD  = preload("res://games/jonathan_platformer/tools/DeathCounter.gd")
const TORCH_BAR_GD  = preload("res://games/jonathan_platformer/tools/TorchBar.gd")
const CRACKER_SCENE = preload("res://games/jonathan_platformer/tools/Firecracker.tscn")

const MAX_CRACKERS := 5

var _crackers := MAX_CRACKERS
var _cracker_label: Label

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color(0.05, 0.03, 0.12))

	var cm := CanvasModulate.new()
	cm.color = Color(0.22, 0.20, 0.35, 1.0)
	add_child(cm)

	_platform(Vector2(1400, 480), Vector2(2900, 40))
	_platform(Vector2(400,  360), Vector2(190, 24))
	_platform(Vector2(720,  265), Vector2(170, 24))
	_platform(Vector2(1060, 370), Vector2(210, 24))
	_platform(Vector2(1420, 255), Vector2(180, 24))
	_platform(Vector2(1780, 375), Vector2(190, 24))
	_platform(Vector2(2120, 285), Vector2(170, 24))

	_death_zone(Vector2(1400, 620), Vector2(3200, 40))

	_build_ui()

func _exit_tree() -> void:
	RenderingServer.set_default_clear_color(Color(0.3, 0.3, 0.3))

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.keycode == KEY_F \
			and event.pressed and not event.echo:
		_throw_cracker()

func _throw_cracker() -> void:
	if _crackers <= 0:
		return
	var player := get_tree().current_scene.get_node_or_null("Player")
	if player == null:
		return
	_crackers -= 1
	_update_cracker_label()
	var dir: float = sign(get_global_mouse_position().x - player.global_position.x)
	if dir == 0.0:
		dir = 1.0
	var fc := CRACKER_SCENE.instantiate()
	fc.global_position = player.global_position + Vector2(0.0, -18.0)
	fc.init(float(dir))
	add_child(fc)

func _platform(pos: Vector2, size: Vector2) -> void:
	var body := StaticBody2D.new()
	body.position = pos
	var cs   := CollisionShape2D.new()
	var rect := RectangleShape2D.new()
	rect.size = size
	cs.shape = rect
	body.add_child(cs)
	var vis := Polygon2D.new()
	vis.color = Color(0.18, 0.15, 0.24)
	var hw := size.x * 0.5
	var hh := size.y * 0.5
	vis.polygon = PackedVector2Array([
		Vector2(-hw, -hh), Vector2(hw, -hh), Vector2(hw, hh), Vector2(-hw, hh)
	])
	body.add_child(vis)
	add_child(body)

func _death_zone(pos: Vector2, size: Vector2) -> void:
	var area := Area2D.new()
	area.position = pos
	var cs   := CollisionShape2D.new()
	var rect := RectangleShape2D.new()
	rect.size = size
	cs.shape = rect
	area.add_child(cs)
	area.body_entered.connect(func(b: Node2D) -> void:
		if b.name == "Player":
			PlayerDeath.trigger(get_tree()))
	add_child(area)

func _build_ui() -> void:
	var ui := CanvasLayer.new()
	add_child(ui)

	var btn := Button.new()
	btn.text = "Back to Menu"
	btn.offset_left = 8.0
	btn.offset_top = 8.0
	btn.offset_right = 130.0
	btn.offset_bottom = 40.0
	btn.pressed.connect(func() -> void:
		get_tree().change_scene_to_file("res://launcher/Launcher.tscn"))
	ui.add_child(btn)

	_cracker_label = Label.new()
	_cracker_label.set_anchors_preset(Control.PRESET_TOP_RIGHT)
	_cracker_label.offset_left   = -200.0
	_cracker_label.offset_top    = 8.0
	_cracker_label.offset_right  = -8.0
	_cracker_label.offset_bottom = 32.0
	_cracker_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	_update_cracker_label()
	ui.add_child(_cracker_label)

	var hint := Label.new()
	hint.set_anchors_preset(Control.PRESET_BOTTOM_RIGHT)
	hint.offset_left   = -220.0
	hint.offset_top    = -36.0
	hint.offset_right  = -8.0
	hint.offset_bottom = -8.0
	hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
	hint.text = "[F] throw firecracker"
	hint.modulate = Color(1, 1, 1, 0.5)
	ui.add_child(hint)

	var death_ctr := Label.new()
	death_ctr.set_script(DEATH_CTR_GD)
	death_ctr.set_anchors_preset(Control.PRESET_CENTER_TOP)
	death_ctr.offset_left   = -80.0
	death_ctr.offset_top    = 8.0
	death_ctr.offset_right  = 80.0
	death_ctr.offset_bottom = 32.0
	ui.add_child(death_ctr)

func _update_cracker_label() -> void:
	if _cracker_label != null:
		_cracker_label.text = "Firecrackers: %d / %d" % [_crackers, MAX_CRACKERS]
