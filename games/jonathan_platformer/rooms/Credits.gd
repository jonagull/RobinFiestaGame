extends CanvasLayer

func _ready() -> void:
	layer = 0
	var vp := get_viewport().get_visible_rect().size
	_build_background(vp)
	_build_content(vp)
	_build_back_button(vp)
	_spawn_sparkles(vp)
	_fade_in()

func _build_background(vp: Vector2) -> void:
	var bg := ColorRect.new()
	bg.color = Color(0.06, 0.05, 0.10)
	bg.size = vp
	add_child(bg)

	# Subtle gradient overlay at bottom
	var grad_rect := ColorRect.new()
	grad_rect.color = Color(0.45, 0.20, 0.85, 0.12)
	grad_rect.size = Vector2(vp.x, 200)
	grad_rect.position = Vector2(0, vp.y - 200)
	add_child(grad_rect)

func _build_content(vp: Vector2) -> void:
	var cx := vp.x * 0.5

	# "THANK YOU" title
	var title := Label.new()
	title.text = "THANK YOU"
	title.add_theme_font_size_override("font_size", 78)
	title.add_theme_color_override("font_color", Color(1.0, 0.88, 0.28))
	title.position = Vector2(cx - 240, 60)
	add_child(title)

	# Decorative line
	var line := ColorRect.new()
	line.color = Color(0.55, 0.28, 0.92, 0.6)
	line.size = Vector2(500, 2)
	line.position = Vector2(cx - 250, 168)
	add_child(line)

	# Subtitle
	var sub := Label.new()
	sub.text = "The migration monster has been defeated."
	sub.add_theme_font_size_override("font_size", 22)
	sub.add_theme_color_override("font_color", Color(0.75, 0.70, 1.0))
	sub.position = Vector2(cx - 220, 184)
	add_child(sub)

	# Main farewell message
	var msg := Label.new()
	msg.text = (
		"To our CTO, who is moving on to new adventures —\n\n"
		+ "Thank you for guiding us through every migration, every late-night deploy,\n"
		+ "every hard decision, and every moment that made this team what it is.\n\n"
		+ "You didn't just lead a team. You built something that will last.\n\n"
		+ "We'll miss you. Good luck out there. Go ship something great."
	)
	msg.add_theme_font_size_override("font_size", 18)
	msg.add_theme_color_override("font_color", Color(0.90, 0.88, 1.0))
	msg.position = Vector2(cx - 320, 240)
	msg.size = Vector2(640, 320)
	msg.autowrap_mode = 3
	add_child(msg)

	# Signature
	var sig := Label.new()
	sig.text = "— The Team  ❤"
	sig.add_theme_font_size_override("font_size", 22)
	sig.add_theme_color_override("font_color", Color(0.65, 0.45, 1.0))
	sig.position = Vector2(cx + 60, 540)
	add_child(sig)

func _build_back_button(vp: Vector2) -> void:
	var btn := Button.new()
	btn.text = "Back to Game"
	btn.add_theme_font_size_override("font_size", 16)
	btn.size = Vector2(180, 44)
	btn.position = Vector2(vp.x * 0.5 - 90, vp.y - 70)
	btn.pressed.connect(func() -> void:
		get_tree().change_scene_to_file(
			"res://games/jonathan_platformer/Platformer.tscn")
	)
	add_child(btn)

func _spawn_sparkles(vp: Vector2) -> void:
	var colors := [
		Color(1.0, 0.88, 0.28, 0.9),
		Color(0.65, 0.35, 1.0, 0.8),
		Color(0.35, 0.85, 1.0, 0.8),
		Color(1.0, 0.50, 0.20, 0.8),
	]
	for i in 24:
		var dot := ColorRect.new()
		var s := randf_range(2.5, 7.0)
		dot.color = colors[i % colors.size()]
		dot.size = Vector2(s, s)
		dot.position = Vector2(randf_range(20, vp.x - 20), randf_range(20, vp.y - 20))
		add_child(dot)
		var tw := create_tween().set_loops()
		var on_dur  := randf_range(0.6, 2.0)
		var off_dur := randf_range(0.6, 2.0)
		tw.tween_property(dot, "modulate:a", 1.0, on_dur)
		tw.tween_property(dot, "modulate:a", 0.05, off_dur)

func _fade_in() -> void:
	# Start white (matching the chest burst) and fade to reveal credits
	var overlay := ColorRect.new()
	overlay.color = Color(1.0, 0.95, 0.70, 1.0)
	var vp := get_viewport().get_visible_rect().size
	overlay.size = vp
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(overlay)
	var tw := create_tween()
	tw.set_trans(Tween.TRANS_SINE)
	tw.tween_property(overlay, "color:a", 0.0, 1.4)
	tw.tween_callback(overlay.queue_free)
