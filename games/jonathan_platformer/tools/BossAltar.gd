extends Area2D

@export var cat_group: String = "boss_cat"

var _used      := false
var _nearby    := false
var _pulse_t   := 0.0

@onready var _prompt: Label        = $PromptLabel
@onready var _glow:   PointLight2D = $Glow
@onready var _gem:    Polygon2D    = $Gem

func _ready() -> void:
	add_to_group("boss_altar")
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	_prompt.visible = false
	_build_glow()

func _build_glow() -> void:
	var gradient := Gradient.new()
	gradient.set_color(0, Color(0.7, 0.2, 1.0, 1.0))
	gradient.set_color(1, Color(0.7, 0.2, 1.0, 0.0))
	var tex := GradientTexture2D.new()
	tex.gradient  = gradient
	tex.fill      = GradientTexture2D.FILL_RADIAL
	tex.fill_from = Vector2(0.5, 0.5)
	tex.fill_to   = Vector2(1.0, 0.5)
	tex.width     = 256
	tex.height    = 256
	_glow.texture       = tex
	_glow.texture_scale = 1.8
	_glow.energy        = 0.5

func _process(delta: float) -> void:
	if _used:
		return
	_pulse_t += delta * 1.8
	_glow.energy = 0.45 + sin(_pulse_t) * 0.18
	_gem.color   = Color(0.55 + sin(_pulse_t) * 0.12, 0.08, 0.9 - sin(_pulse_t) * 0.1)

func _unhandled_input(event: InputEvent) -> void:
	if not _nearby or _used:
		return
	if event.is_action_pressed("interact"):
		_activate()
		get_viewport().set_input_as_handled()

func _activate() -> void:
	_used = true
	_prompt.visible = false
	var tween := create_tween()
	tween.set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_OUT)
	tween.tween_property(_glow, "energy", 4.0, 0.1)
	tween.tween_property(_glow, "energy", 0.0, 1.2)
	tween.parallel().tween_property(_gem, "color", Color(0.15, 0.05, 0.25), 1.2)
	for door in get_tree().get_nodes_in_group("big_door"):
		door.boss_lock()
	for cat in get_tree().get_nodes_in_group(cat_group):
		cat.pre_wake()
	# Staged dialog: first reaction, then tactical hint as the cat fully wakes
	get_tree().create_timer(1.5).timeout.connect(func() -> void:
		var adv := get_tree().get_first_node_in_group("boss_advisor")
		if adv:
			adv.say("Oh no... the migration monster!")
	)
	get_tree().create_timer(6.0).timeout.connect(func() -> void:
		var adv := get_tree().get_first_node_in_group("boss_advisor")
		if adv:
			adv.say("He's shielding up! Break the pillars!")
	)

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player" and not _used:
		_nearby = true
		_prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_nearby = false
		_prompt.visible = false
