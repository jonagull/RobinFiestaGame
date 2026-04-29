extends StaticBody2D

@export var lever_count: int = 4
@export var door_group: String = "big_door"

const DOOR_H    := 280
const SLIDE_Y   := -300.0
const BTN_READY := Color(0.1, 0.7, 0.2)
const BTN_IDLE  := Color(0.6, 0.18, 0.18)

var _pulled        := 0
var _open          := false
var _player_nearby := false
var _lights        := []

@onready var _shape_node:    CollisionShape2D = $CollisionShape2D
@onready var _button_visual: Polygon2D        = $ButtonVisual
@onready var _button_area:   Area2D           = $ButtonArea
@onready var _prompt:        Label            = $PromptLabel

func _ready() -> void:
	add_to_group(door_group)
	_button_area.body_entered.connect(_on_body_entered)
	_button_area.body_exited.connect(_on_body_exited)
	_prompt.visible = false
	_build_indicators()

func _build_indicators() -> void:
	var spacing := 13.0
	var total_w: float = (lever_count - 1) * spacing
	var tex := _make_light_texture()
	for i in lever_count:
		var light := PointLight2D.new()
		var cx: float = -total_w / 2.0 + i * spacing
		light.position = Vector2(cx, -70.0)
		light.texture = tex
		light.texture_scale = 0.14
		light.energy = 0.2
		light.color = Color(0.3, 0.3, 0.3)
		add_child(light)
		_lights.append(light)

func _make_light_texture() -> GradientTexture2D:
	var gradient := Gradient.new()
	gradient.set_color(0, Color(1.0, 1.0, 1.0, 1.0))
	gradient.set_color(1, Color(1.0, 1.0, 1.0, 0.0))
	var tex := GradientTexture2D.new()
	tex.gradient = gradient
	tex.fill = GradientTexture2D.FILL_RADIAL
	tex.fill_from = Vector2(0.5, 0.5)
	tex.fill_to   = Vector2(1.0, 0.5)
	tex.width  = 64
	tex.height = 64
	return tex

func _unhandled_input(event: InputEvent) -> void:
	if not _player_nearby or _open:
		return
	if event.is_action_pressed("interact"):
		_try_open()
		get_viewport().set_input_as_handled()

func _try_open() -> void:
	if _pulled >= lever_count:
		_slide_open()
	else:
		var tween := create_tween()
		tween.tween_property(_button_visual, "color", Color(1.0, 0.1, 0.1), 0.08)
		tween.tween_property(_button_visual, "color", BTN_IDLE, 0.3)

func on_lever_pulled() -> void:
	if _open or _pulled >= lever_count:
		return
	if _pulled < _lights.size():
		var light: PointLight2D = _lights[_pulled]
		light.energy = 2.2
		light.color = Color(0.2, 1.0, 0.35)
	_pulled += 1
	if _pulled >= lever_count:
		_button_visual.color = BTN_READY

func _slide_open() -> void:
	_open = true
	_prompt.visible = false
	_shape_node.set_deferred("disabled", true)
	var tween := create_tween()
	tween.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tween.tween_property(self, "position:y", position.y + SLIDE_Y, 1.4)

func boss_lock() -> void:
	if not _open:
		return
	_open = false
	_prompt.visible = false
	_shape_node.set_deferred("disabled", false)
	var tween := create_tween()
	tween.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tween.tween_property(self, "position:y", position.y - SLIDE_Y, 1.4)

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player" and not _open:
		_player_nearby = true
		_prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = false
		_prompt.visible = false
