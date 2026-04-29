extends Area2D

const LINES := [
	"You did it. The migration monster is defeated.",
	"I've watched you fight through every obstacle, every deploy, every 3am incident.",
	"This game was made for our CTO, who is moving on to new adventures.",
	"Thank you for everything you built here. The team wouldn't be the same without you.",
	"Now open that chest. You've earned it.",
]

var _player_nearby := false
var _in_dialogue   := false
var _line_idx      := 0

var _canvas:   CanvasLayer = null
var _panel:    Control     = null
var _text_lbl: Label       = null
var _hint_lbl: Label       = null
var _prompt:   Label       = null

func _ready() -> void:
	_build_visual()
	_build_ui()
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	# Slide in with a small bounce
	modulate.a = 0.0
	var tw := create_tween()
	tw.tween_property(self, "modulate:a", 1.0, 0.6)

func _build_visual() -> void:
	# Body (same blue as world NPC)
	var body := Polygon2D.new()
	body.color = Color(0.35, 0.55, 0.85, 1)
	body.polygon = PackedVector2Array([-14, 0, 14, 0, 14, -52, -14, -52])
	add_child(body)
	# Head
	var head := Polygon2D.new()
	head.color = Color(0.82, 0.65, 0.50, 1)
	head.polygon = PackedVector2Array([-11, -52, 11, -52, 11, -72, -11, -72])
	add_child(head)
	# Prompt
	_prompt = Label.new()
	_prompt.name = "PromptLabel"
	_prompt.text = "[E] talk"
	_prompt.visible = false
	_prompt.add_theme_font_size_override("font_size", 14)
	_prompt.position = Vector2(-22, -94)
	add_child(_prompt)
	# Collision
	var area_shape := CollisionShape2D.new()
	var rect := RectangleShape2D.new()
	rect.size = Vector2(100, 100)
	area_shape.shape = rect
	area_shape.position = Vector2(0, -36)
	add_child(area_shape)

func _build_ui() -> void:
	_canvas = CanvasLayer.new()
	_canvas.layer = 25
	add_child(_canvas)

	_panel = Control.new()
	_panel.visible = false
	_canvas.add_child(_panel)

	var W := 540.0
	var H := 110.0

	var bg := ColorRect.new()
	bg.color = Color(0.09, 0.07, 0.13, 0.95)
	bg.size = Vector2(W, H)
	_panel.add_child(bg)

	var port := ColorRect.new()
	port.color = Color(0.35, 0.55, 0.85, 1)
	port.size = Vector2(26, 52)
	port.position = Vector2(10, H - 58)
	_panel.add_child(port)

	var div := ColorRect.new()
	div.color = Color(0.55, 0.28, 0.92, 0.7)
	div.size = Vector2(2, H)
	div.position = Vector2(44, 0)
	_panel.add_child(div)

	var name_lbl := Label.new()
	name_lbl.text = "Stranger"
	name_lbl.add_theme_font_size_override("font_size", 11)
	name_lbl.add_theme_color_override("font_color", Color(0.65, 0.45, 1.0))
	name_lbl.position = Vector2(52, 8)
	_panel.add_child(name_lbl)

	var sep := ColorRect.new()
	sep.color = Color(0.4, 0.25, 0.7, 0.35)
	sep.size = Vector2(W - 52, 1)
	sep.position = Vector2(52, 24)
	_panel.add_child(sep)

	_text_lbl = Label.new()
	_text_lbl.add_theme_font_size_override("font_size", 13)
	_text_lbl.add_theme_color_override("font_color", Color(0.93, 0.91, 1.0))
	_text_lbl.position = Vector2(52, 30)
	_text_lbl.size = Vector2(W - 60, H - 44)
	_text_lbl.autowrap_mode = 3
	_panel.add_child(_text_lbl)

	_hint_lbl = Label.new()
	_hint_lbl.text = "[E] continue"
	_hint_lbl.add_theme_font_size_override("font_size", 10)
	_hint_lbl.add_theme_color_override("font_color", Color(0.55, 0.55, 0.75))
	_hint_lbl.position = Vector2(W - 110, H - 18)
	_panel.add_child(_hint_lbl)

func _place_panel() -> void:
	var vp := get_viewport().get_visible_rect().size
	_panel.position = Vector2(vp.x * 0.5 - 270, vp.y - 130)

func _unhandled_input(event: InputEvent) -> void:
	if not _player_nearby:
		return
	if event.is_action_pressed("interact"):
		if _in_dialogue:
			_advance()
		else:
			_start()
		get_viewport().set_input_as_handled()

func _start() -> void:
	_in_dialogue = true
	_line_idx = 0
	_prompt.visible = false
	_place_panel()
	_text_lbl.text = LINES[0]
	_panel.visible = true

func _advance() -> void:
	_line_idx += 1
	if _line_idx >= LINES.size():
		_panel.visible = false
		_in_dialogue = false
		if _player_nearby:
			_prompt.visible = true
		_spawn_chest()
	else:
		_text_lbl.text = LINES[_line_idx]

func _spawn_chest() -> void:
	var scene := load("res://games/jonathan_platformer/tools/TreasureChest.tscn")
	if scene:
		var chest: Node2D = scene.instantiate()
		chest.global_position = global_position + Vector2(200, 0)
		get_parent().add_child(chest)

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = true
		if not _in_dialogue:
			_prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = false
		if not _in_dialogue:
			_prompt.visible = false
