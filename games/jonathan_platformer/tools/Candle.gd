extends Area2D

var _lit           := false
var _player_nearby := false

@onready var _light:  PointLight2D = $Light
@onready var _prompt: Label        = $PromptLabel

func _ready() -> void:
	_light.energy = 0.0
	_prompt.visible = false
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	# Generate warm candle light texture
	var gradient := Gradient.new()
	gradient.set_color(0, Color(1.0, 0.75, 0.3, 1.0))
	gradient.set_color(1, Color(1.0, 0.75, 0.3, 0.0))
	var tex := GradientTexture2D.new()
	tex.gradient  = gradient
	tex.fill      = GradientTexture2D.FILL_RADIAL
	tex.fill_from = Vector2(0.5, 0.5)
	tex.fill_to   = Vector2(1.0, 0.5)
	tex.width     = 256
	tex.height    = 256
	_light.texture = tex

func _unhandled_input(event: InputEvent) -> void:
	if _lit or not _player_nearby:
		return
	if event.is_action_pressed("interact"):
		_light_candle()
		get_viewport().set_input_as_handled()

func _light_candle() -> void:
	_lit = true
	_prompt.visible = false
	var tween := create_tween()
	tween.tween_property(_light, "texture_scale", 4.0, 0.8).set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_OUT)
	tween.parallel().tween_property(_light, "energy", 1.4, 0.8)

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player" and not _lit:
		_player_nearby = true
		_prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = false
		_prompt.visible = false
