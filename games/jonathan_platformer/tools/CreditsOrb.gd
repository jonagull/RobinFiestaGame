extends Area2D

var _nearby := false
var _prompt: Label = null

func _ready() -> void:
	visible = false
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	_build()

func _build() -> void:
	var tex := _radial_tex()

	var glow := PointLight2D.new()
	glow.name = "Glow"
	glow.texture = tex
	glow.texture_scale = 1.2
	glow.energy = 2.0
	glow.color = Color(0.2, 0.1, 0.4)
	add_child(glow)

	var dot := Polygon2D.new()
	var pts := PackedVector2Array()
	for i in 20:
		var a := TAU * i / 20.0
		pts.append(Vector2(cos(a), sin(a)) * 16.0)
	dot.polygon = pts
	dot.color = Color(0.12, 0.06, 0.22)
	add_child(dot)

	_prompt = Label.new()
	_prompt.text = "[E]"
	_prompt.add_theme_font_size_override("font_size", 14)
	_prompt.add_theme_color_override("font_color", Color(0.75, 0.65, 0.9))
	_prompt.position = Vector2(-55, -52)
	_prompt.visible = false
	add_child(_prompt)

	var tw := glow.create_tween().set_loops()
	tw.tween_property(glow, "energy", 3.5, 1.3).set_trans(Tween.TRANS_SINE)
	tw.tween_property(glow, "energy", 1.5, 1.3).set_trans(Tween.TRANS_SINE)

# Called by NPC via opens_on_finish
func open() -> void:
	visible = true
	modulate.a = 0.0
	var tw := create_tween()
	tw.tween_property(self, "modulate:a", 1.0, 1.2)

func _unhandled_input(event: InputEvent) -> void:
	if not _nearby:
		return
	if event.is_action_pressed("interact"):
		get_viewport().set_input_as_handled()
		_flash_and_go()

func _flash_and_go() -> void:
	_nearby = false
	_prompt.visible = false
	var canvas := CanvasLayer.new()
	canvas.layer = 100
	get_tree().current_scene.add_child(canvas)
	var overlay := ColorRect.new()
	overlay.color = Color(0.0, 0.0, 0.0, 0.0)
	overlay.size = get_viewport().get_visible_rect().size
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	canvas.add_child(overlay)
	var tw := canvas.create_tween()
	tw.tween_property(overlay, "color:a", 1.0, 0.8).set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_IN)
	tw.tween_callback(func() -> void:
		get_tree().change_scene_to_file(
			"res://games/jonathan_platformer/CreditsScene.tscn")
	)

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player" and visible:
		_nearby = true
		_prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_nearby = false
		_prompt.visible = false

func _radial_tex() -> GradientTexture2D:
	var g := Gradient.new()
	g.set_color(0, Color(1, 1, 1, 1))
	g.set_color(1, Color(1, 1, 1, 0))
	var t := GradientTexture2D.new()
	t.gradient = g
	t.fill = GradientTexture2D.FILL_RADIAL
	t.fill_from = Vector2(0.5, 0.5)
	t.fill_to = Vector2(1.0, 0.5)
	t.width = 256
	t.height = 256
	return t
