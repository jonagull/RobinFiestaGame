extends Area2D

@onready var _light:  PointLight2D = $Light
@onready var _prompt: Label        = $PromptLabel
@onready var _flames: Node2D       = $Flames

var _player_nearby := false
var _lit           := false

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	_prompt.visible = false
	_flames.visible = false
	_build_light()

func _build_light() -> void:
	var gradient := Gradient.new()
	gradient.set_color(0, Color(1.0, 0.65, 0.2, 1.0))
	gradient.set_color(1, Color(1.0, 0.65, 0.2, 0.0))
	var tex := GradientTexture2D.new()
	tex.gradient  = gradient
	tex.fill      = GradientTexture2D.FILL_RADIAL
	tex.fill_from = Vector2(0.5, 0.5)
	tex.fill_to   = Vector2(1.0, 0.5)
	tex.width     = 512
	tex.height    = 512
	_light.texture       = tex
	_light.texture_scale = 6.0
	_light.energy        = 0.0

func _unhandled_input(event: InputEvent) -> void:
	if not _player_nearby or _lit:
		return
	if event.is_action_pressed("interact"):
		_try_light()
		get_viewport().set_input_as_handled()

func _try_light() -> void:
	var player := _get_nearby_player()
	if player == null:
		return
	var plight := player.get_node_or_null("PlayerLight")
	if plight == null or not plight._has_torch or plight._fuel <= 0.0:
		return
	plight._fuel = maxf(0.0, plight._fuel - plight.torch_duration * 0.3)
	_lit = true
	_prompt.visible = false
	_flames.visible = true
	var tween := create_tween()
	tween.set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_OUT)
	tween.tween_property(_light, "energy", 6.0, 0.25)
	tween.tween_property(_light, "energy", 3.5, 0.7)
	_cascade_flames()

func _cascade_flames() -> void:
	var flames := get_tree().get_nodes_in_group("chain_flame")
	flames.sort_custom(func(a, b):
		return global_position.distance_to(a.global_position) < global_position.distance_to(b.global_position)
	)
	for i in flames.size():
		flames[i].ignite(i * 0.4)

func _on_body_entered(body: Node2D) -> void:
	var plight := body.get_node_or_null("PlayerLight")
	if plight == null:
		return
	_player_nearby = true
	if not _lit:
		_prompt.visible = plight._has_torch and plight._fuel > 0.0

func _on_body_exited(body: Node2D) -> void:
	if body.get_node_or_null("PlayerLight") != null:
		_player_nearby = false
		_prompt.visible = false

func _get_nearby_player() -> Node2D:
	for body in get_overlapping_bodies():
		if body.get_node_or_null("PlayerLight") != null:
			return body
	return null
