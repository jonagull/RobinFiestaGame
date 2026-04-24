extends Area2D

var _player_nearby := false
var _picked_up     := false

@onready var prompt: Label = $PromptLabel

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	prompt.visible = false

func _unhandled_input(event: InputEvent) -> void:
	if not _player_nearby or _picked_up:
		return
	if event.is_action_pressed("interact"):
		_collect()
		get_viewport().set_input_as_handled()

func _collect() -> void:
	_picked_up = true
	prompt.visible = false
	var balls := get_tree().get_nodes_in_group("golf_ball")
	if balls.size() > 0:
		balls[0].unlock()
	# Orb pops up then shrinks away
	var tween := create_tween().set_parallel(true)
	tween.tween_property($Orb, "scale", Vector2(1.6, 1.6), 0.12).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tween.tween_property($OrbGlow, "scale", Vector2(2.0, 2.0), 0.12).set_ease(Tween.EASE_OUT)
	await get_tree().create_timer(0.12).timeout
	var tween2 := create_tween().set_parallel(true)
	tween2.tween_property($Orb, "scale", Vector2.ZERO, 0.25).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_IN)
	tween2.tween_property($OrbGlow, "scale", Vector2.ZERO, 0.25).set_ease(Tween.EASE_IN)
	tween2.tween_callback(func(): visible = false).set_delay(0.25)
	_spawn_pickup_text()

func _spawn_pickup_text() -> void:
	var lbl := Label.new()
	lbl.text = "GOLF BALL GET!"
	lbl.add_theme_font_size_override("font_size", 20)
	lbl.add_theme_color_override("font_color", Color(1.0, 0.95, 0.3))
	lbl.add_theme_color_override("font_shadow_color", Color(0, 0, 0, 0.8))
	lbl.add_theme_constant_override("shadow_offset_x", 2)
	lbl.add_theme_constant_override("shadow_offset_y", 2)
	lbl.position = global_position + Vector2(-60, -60)
	# Use top-level so it isn't affected if this node hides
	lbl.top_level = true
	get_tree().current_scene.add_child(lbl)
	var tween := lbl.create_tween().set_parallel(true)
	tween.tween_property(lbl, "position:y", lbl.position.y - 70, 1.2).set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_OUT)
	tween.tween_property(lbl, "modulate:a", 0.0, 1.2).set_delay(0.4)
	tween.tween_callback(lbl.queue_free).set_delay(1.2)

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player" and not _picked_up:
		_player_nearby = true
		prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = false
		prompt.visible = false
