extends Node2D

const DEATH_CTR_GD = preload("res://games/jonathan_platformer/tools/DeathCounter.gd")
const TORCH_BAR_GD = preload("res://games/jonathan_platformer/tools/TorchBar.gd")

const FLOOR_COLOR    := Color(0.55, 0.50, 0.44)
const PLATFORM_COLOR := Color(0.48, 0.43, 0.38)

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color(0.72, 0.67, 0.60))

	_platform(Vector2(1700, 480), Vector2(3800, 60))
	_platform(Vector2(520,  360), Vector2(200, 24))
	_platform(Vector2(870,  265), Vector2(180, 24))
	_platform(Vector2(1220, 375), Vector2(220, 24))
	_platform(Vector2(1590, 268), Vector2(200, 24))
	_platform(Vector2(1940, 390), Vector2(160, 24))
	_platform(Vector2(2280, 305), Vector2(200, 24))

	_death_zone(Vector2(1700, 640), Vector2(4400, 40))

	_build_ui()

func _exit_tree() -> void:
	RenderingServer.set_default_clear_color(Color(0.3, 0.3, 0.3))

func _platform(pos: Vector2, size: Vector2) -> void:
	var body := StaticBody2D.new()
	body.position = pos
	var cs   := CollisionShape2D.new()
	var rect := RectangleShape2D.new()
	rect.size = size
	cs.shape = rect
	body.add_child(cs)
	var vis := Polygon2D.new()
	vis.color = FLOOR_COLOR if size.y >= 40 else PLATFORM_COLOR
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

	var torch_bar := Control.new()
	torch_bar.set_script(TORCH_BAR_GD)
	torch_bar.set_anchors_preset(Control.PRESET_FULL_RECT)
	ui.add_child(torch_bar)

	var death_ctr := Label.new()
	death_ctr.set_script(DEATH_CTR_GD)
	death_ctr.set_anchors_preset(Control.PRESET_CENTER_TOP)
	death_ctr.offset_left   = -80.0
	death_ctr.offset_top    = 8.0
	death_ctr.offset_right  = 80.0
	death_ctr.offset_bottom = 32.0
	ui.add_child(death_ctr)
