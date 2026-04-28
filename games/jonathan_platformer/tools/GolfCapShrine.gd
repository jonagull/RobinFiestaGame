extends Area2D

var _player_nearby := false

@onready var _prompt: Label    = $PromptLabel
@onready var _cap_visual: Node2D = $CapVisual

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	_prompt.visible = false

func _unhandled_input(event: InputEvent) -> void:
	if not _player_nearby or GameData.has_golf_cap:
		return
	if event.is_action_pressed("interact"):
		_pickup()
		get_viewport().set_input_as_handled()

func _pickup() -> void:
	GameData.has_golf_cap = true
	GameData.selected_slot = 1
	_prompt.visible = false
	_show_message()
	var tween := create_tween()
	tween.tween_property(_cap_visual, "scale", Vector2.ZERO, 0.35).set_trans(Tween.TRANS_BACK)
	tween.tween_callback(queue_free)

func _show_message() -> void:
	var ui := get_tree().current_scene.get_node_or_null("UI")
	if ui == null:
		return
	var label := Label.new()
	label.text = "Golf Cap Get!"
	label.add_theme_font_size_override("font_size", 28)
	label.set_anchors_preset(Control.PRESET_CENTER)
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	ui.add_child(label)
	var tween := label.create_tween()
	tween.tween_property(label, "position:y", label.position.y - 60, 1.4)
	tween.parallel().tween_property(label, "modulate:a", 0.0, 1.4).set_delay(0.5)
	tween.tween_callback(label.queue_free)

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player" and not GameData.has_golf_cap:
		_player_nearby = true
		_prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = false
		_prompt.visible = false
