extends Area2D

func _ready() -> void:
	body_entered.connect(_on_body_entered)

func _on_body_entered(body: Node2D) -> void:
	var light := body.get_node_or_null("PlayerLight")
	if light == null or not light.has_method("give_torch"):
		return
	light.give_torch()
	_show_message()
	queue_free()

func _show_message() -> void:
	var ui := get_tree().current_scene.get_node_or_null("UI")
	if ui == null:
		return
	var label := Label.new()
	label.text = "Torch picked up!"
	label.add_theme_font_size_override("font_size", 28)
	label.set_anchors_preset(Control.PRESET_CENTER)
	label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	ui.add_child(label)
	var tween := label.create_tween()
	tween.tween_property(label, "position:y", label.position.y - 60, 1.4)
	tween.parallel().tween_property(label, "modulate:a", 0.0, 1.4).set_delay(0.5)
	tween.tween_callback(label.queue_free)
