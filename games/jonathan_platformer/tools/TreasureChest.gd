extends Area2D

var _player_nearby := false
var _opened        := false
var _lid_pivot:    Node2D    = null
var _inner_glow:   PointLight2D = null
var _prompt:       Label     = null

func _ready() -> void:
	_build_visual()
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	# Collision shape
	var cs := CollisionShape2D.new()
	var rect := RectangleShape2D.new()
	rect.size = Vector2(64, 44)
	cs.shape = rect
	cs.position = Vector2(0, -22)
	add_child(cs)
	# Fade in
	modulate.a = 0.0
	var tw := create_tween()
	tw.tween_property(self, "modulate:a", 1.0, 0.7)

func _build_visual() -> void:
	# Base feet
	var base := Polygon2D.new()
	base.color = Color(0.38, 0.24, 0.07)
	base.polygon = PackedVector2Array([-32, 0, 32, 0, 28, -6, -28, -6])
	add_child(base)

	# Body
	var body := Polygon2D.new()
	body.color = Color(0.58, 0.38, 0.12)
	body.polygon = PackedVector2Array([-28, -6, 28, -6, 28, -38, -28, -38])
	add_child(body)

	# Horizontal metal band on body
	var band := Polygon2D.new()
	band.color = Color(0.78, 0.62, 0.18)
	band.polygon = PackedVector2Array([-28, -20, 28, -20, 28, -25, -28, -25])
	add_child(band)

	# Lock plate
	var lock := Polygon2D.new()
	lock.color = Color(0.82, 0.66, 0.15)
	lock.polygon = PackedVector2Array([-9, -32, 9, -32, 9, -38, -9, -38])
	add_child(lock)

	# Lid pivot at top of body
	_lid_pivot = Node2D.new()
	_lid_pivot.position = Vector2(0, -38)
	add_child(_lid_pivot)

	# Lid (hangs down from pivot)
	var lid := Polygon2D.new()
	lid.color = Color(0.62, 0.41, 0.13)
	lid.polygon = PackedVector2Array([-30, 0, 30, 0, 26, -17, -26, -17])
	_lid_pivot.add_child(lid)

	# Lid metal band
	var lid_band := Polygon2D.new()
	lid_band.color = Color(0.78, 0.62, 0.18)
	lid_band.polygon = PackedVector2Array([-26, -6, 26, -6, 26, -11, -26, -11])
	_lid_pivot.add_child(lid_band)

	# Lid lock clasp
	var clasp := Polygon2D.new()
	clasp.color = Color(0.82, 0.66, 0.15)
	clasp.polygon = PackedVector2Array([-7, -2, 7, -2, 9, -15, -9, -15])
	_lid_pivot.add_child(clasp)

	# Inner glow (lit when opened)
	_inner_glow = PointLight2D.new()
	_inner_glow.position = Vector2(0, -22)
	_inner_glow.color = Color(1.0, 0.90, 0.40)
	_inner_glow.energy = 0.0
	_inner_glow.texture_scale = 1.2
	_inner_glow.texture = _radial_tex()
	add_child(_inner_glow)

	# Prompt label
	_prompt = Label.new()
	_prompt.text = "[E] open"
	_prompt.visible = false
	_prompt.add_theme_font_size_override("font_size", 14)
	_prompt.position = Vector2(-28, -72)
	add_child(_prompt)

func _unhandled_input(event: InputEvent) -> void:
	if not _player_nearby or _opened:
		return
	if event.is_action_pressed("interact"):
		_open()
		get_viewport().set_input_as_handled()

func _open() -> void:
	_opened = true
	_prompt.visible = false

	# Lid swings open
	var tw := create_tween()
	tw.set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tw.tween_property(_lid_pivot, "rotation_degrees", -115.0, 0.55)

	# Inner glow flares up
	var gtw := create_tween()
	gtw.tween_property(_inner_glow, "energy", 3.5, 0.4).set_delay(0.3)

	# Confetti shards burst out
	get_tree().create_timer(0.3).timeout.connect(_spawn_confetti)

	# Full screen burst → credits
	get_tree().create_timer(1.0).timeout.connect(_burst_to_credits)

func _spawn_confetti() -> void:
	var colors := [Color(1.0, 0.85, 0.2), Color(0.7, 0.2, 1.0), Color(0.2, 0.85, 1.0), Color(1.0, 0.4, 0.2)]
	for i in 22:
		var shard := Polygon2D.new()
		shard.color = colors[i % colors.size()]
		var s := randf_range(4.0, 11.0)
		shard.polygon = PackedVector2Array([Vector2(-s*0.4, 0), Vector2(s*0.4, 0), Vector2(0, -s*1.8)])
		shard.global_position = global_position + Vector2(0, -30)
		get_parent().add_child(shard)
		var angle := -PI * 0.85 + randf_range(0.0, PI * 0.7)
		var speed := randf_range(200.0, 480.0)
		var tw := create_tween().set_parallel(true)
		tw.tween_property(shard, "global_position",
			shard.global_position + Vector2(cos(angle), sin(angle)) * speed, 1.3)
		tw.tween_property(shard, "modulate:a", 0.0, 1.0).set_delay(0.35)
		tw.tween_property(shard, "rotation", randf_range(-TAU, TAU), 1.3)
		tw.tween_callback(shard.queue_free).set_delay(1.4)

func _burst_to_credits() -> void:
	var canvas := CanvasLayer.new()
	canvas.layer = 100
	get_tree().current_scene.add_child(canvas)

	var overlay := ColorRect.new()
	overlay.color = Color(1.0, 0.95, 0.70, 0.0)
	var vp := get_viewport().get_visible_rect().size
	overlay.size = vp
	canvas.add_child(overlay)

	var tw := create_tween()
	tw.set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_IN)
	tw.tween_property(overlay, "color:a", 1.0, 0.9)
	tw.tween_callback(func() -> void:
		get_tree().change_scene_to_file(
			"res://games/jonathan_platformer/rooms/Credits.tscn")
	)

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player" and not _opened:
		_player_nearby = true
		_prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = false
		if not _opened:
			_prompt.visible = false

func _radial_tex() -> GradientTexture2D:
	var g := Gradient.new()
	g.set_color(0, Color(1, 1, 1, 1))
	g.set_color(1, Color(1, 1, 1, 0))
	var t := GradientTexture2D.new()
	t.gradient = g
	t.fill = GradientTexture2D.FILL_RADIAL
	t.fill_from = Vector2(0.5, 0.5)
	t.fill_to   = Vector2(1.0, 0.5)
	t.width  = 128
	t.height = 128
	return t
