extends Node2D

const SPEED     := 400.0
const FUSE_TIME := 0.85
const BLAST_R   := 100.0

var _dir      := 1.0
var _timer    := FUSE_TIME
var _done     := false
var _glow: PointLight2D
var _spark_t  := 0.0

func init(direction: float) -> void:
	_dir = direction

func _process(delta: float) -> void:
	if _done:
		return
	_spark_t += delta * 18.0
	global_position.x += _dir * SPEED * delta
	_timer -= delta
	# Spark flicker
	if _glow != null:
		_glow.energy = 0.6 + sin(_spark_t) * 0.3
	if _timer <= 0.0:
		_explode()

func _explode() -> void:
	_done = true
	for ghost in get_tree().get_nodes_in_group("ghost"):
		if global_position.distance_to(ghost.global_position) < BLAST_R:
			ghost.stun()
	if _glow != null:
		_glow.energy = 4.0
		_glow.texture_scale = 1.6
	var tw := create_tween()
	tw.tween_property(_glow, "energy", 0.0, 0.35)
	tw.tween_callback(queue_free)

func _ready() -> void:
	var tex := _radial_tex()

	var body := Polygon2D.new()
	body.color = Color(1.0, 0.75, 0.15)
	body.polygon = _oval(5, 8)
	add_child(body)

	_glow = PointLight2D.new()
	_glow.color = Color(1.0, 0.55, 0.1)
	_glow.energy = 0.7
	_glow.texture_scale = 0.45
	_glow.texture = tex
	add_child(_glow)

func _oval(rx: float, ry: float, n: int = 8) -> PackedVector2Array:
	var arr := PackedVector2Array()
	for i in n:
		var a := TAU * i / n
		arr.append(Vector2(cos(a) * rx, sin(a) * ry))
	return arr

func _radial_tex() -> GradientTexture2D:
	var g := Gradient.new()
	g.set_color(0, Color(1, 1, 1, 1))
	g.set_color(1, Color(1, 1, 1, 0))
	var t := GradientTexture2D.new()
	t.gradient = g
	t.fill = GradientTexture2D.FILL_RADIAL
	t.fill_from = Vector2(0.5, 0.5)
	t.fill_to = Vector2(1.0, 0.5)
	t.width = 128
	t.height = 128
	return t
