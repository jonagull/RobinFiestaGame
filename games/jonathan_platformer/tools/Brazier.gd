extends Area2D

@onready var _light: PointLight2D = $Light
@onready var _prompt: Label = $PromptLabel

var _player_nearby := false

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	_prompt.visible = false
	_build_light()

func _unhandled_input(event: InputEvent) -> void:
	if not _player_nearby:
		return
	if event.is_action_pressed("interact"):
		_light_torch()
		get_viewport().set_input_as_handled()

func _light_torch() -> void:
	var player := _get_nearby_player()
	if player == null:
		return
	var light := player.get_node_or_null("PlayerLight")
	if light == null or not light._has_torch:
		return
	if light._fuel <= 0.0:
		light.light_torch()
	else:
		light.refuel()

func _on_body_entered(body: Node2D) -> void:
	var light := body.get_node_or_null("PlayerLight")
	if light != null:
		_player_nearby = true
		_prompt.visible = light._has_torch

func _on_body_exited(body: Node2D) -> void:
	if body.get_node_or_null("PlayerLight") != null:
		_player_nearby = false
		_prompt.visible = false

func _get_nearby_player() -> Node2D:
	for body in get_overlapping_bodies():
		if body.get_node_or_null("PlayerLight") != null:
			return body
	return null

func _build_light() -> void:
	var gradient := Gradient.new()
	gradient.set_color(0, Color(1.0, 0.65, 0.2, 1.0))
	gradient.set_color(1, Color(1.0, 0.65, 0.2, 0.0))
	var tex := GradientTexture2D.new()
	tex.gradient  = gradient
	tex.fill      = GradientTexture2D.FILL_RADIAL
	tex.fill_from = Vector2(0.5, 0.5)
	tex.fill_to   = Vector2(1.0, 0.5)
	tex.width     = 256
	tex.height    = 256
	_light.texture       = tex
	_light.texture_scale = 2.5
	_light.energy        = 1.2
