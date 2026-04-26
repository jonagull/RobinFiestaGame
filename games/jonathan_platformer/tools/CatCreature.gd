@tool
extends Node2D

const STALK_SPEED   := 55.0    # normal approach speed
const CHARGE_SPEED  := 200.0   # speed when charging
const CHARGE_DIST   := 250.0   # distance that triggers a charge
const STEER_STALK   := 3.2     # how fast it turns while stalking
const STEER_CHARGE  := 0.7     # how fast it turns while charging (low = commits to direction)
const TORCH_DIST    := 300.0
const TORCH_SLOW    := 18.0
const KILL_R        := 58.0
@export var wake_delay: float = 5.5

var _awake    := false
var _wake_t   := 0.0
var _player: Node2D = null
var _velocity := Vector2.ZERO

var _breath_t := 0.0
var _tail_t   := 0.0
var _eye_t    := 0.0

var _body:     Polygon2D
var _eye_l:    Polygon2D
var _eye_r:    Polygon2D
var _pupil_l:  Polygon2D
var _pupil_r:  Polygon2D
var _tail_pivs: Array = []
var _glow_l:   PointLight2D
var _glow_r:   PointLight2D
var _aura:     PointLight2D

func _ready() -> void:
	for child in get_children():
		child.free()
	_build()
	if Engine.is_editor_hint():
		return
	_wake_t = wake_delay
	_player = get_tree().current_scene.get_node_or_null("Player")
	_build_kill()

func _process(delta: float) -> void:
	if Engine.is_editor_hint():
		return
	if not _awake:
		_wake_t -= delta
		if _wake_t <= 0.0:
			_on_wake()
	_breath_t += delta * 0.55
	var tail_mult: float = 1.6 if _awake else 0.35
	_tail_t   += delta * tail_mult
	_eye_t    += delta * 1.7
	_animate()
	if _awake:
		_chase(delta)

func _on_wake() -> void:
	_awake = true
	var tw := create_tween()
	tw.tween_property(_aura, "energy", 0.55, 0.08)
	tw.tween_property(_aura, "energy", 0.06, 0.9)

func _animate() -> void:
	_body.scale = Vector2(1.0 + sin(_breath_t) * 0.022, 1.0 - sin(_breath_t) * 0.014)
	var awake_mult: float = 1.0 if _awake else 0.22
	var eg: float = (0.6 + sin(_eye_t) * 0.25) * awake_mult
	_glow_l.energy = eg
	_glow_r.energy = eg
	if _player != null:
		var d := (_player.global_position - global_position).normalized() * 4.5
		_pupil_l.position = Vector2(-27, -68) + d
		_pupil_r.position = Vector2( 27, -68) + d
	for i in _tail_pivs.size():
		var amp := 12.0 + i * 3.5
		_tail_pivs[i].rotation = deg_to_rad(sin(_tail_t + i * 0.75) * amp)

func _chase(delta: float) -> void:
	if _player == null:
		_player = get_tree().current_scene.get_node_or_null("Player")
		return

	var to    := _player.global_position - global_position
	var dist  := to.length()
	var dir   := to.normalized()
	var spd   := _velocity.length()

	# Steer less when going fast (momentum) — cat commits to a charge direction
	var is_charging := spd > STALK_SPEED * 1.4
	var steer: float       = STEER_CHARGE if is_charging else STEER_STALK

	var target_speed: float = CHARGE_SPEED if dist < CHARGE_DIST else STALK_SPEED

	# Torch slows the cat in dark rooms
	var pl := _player.get_node_or_null("PlayerLight")
	if pl != null and pl._has_torch and pl._fuel > 0.0 and dist < TORCH_DIST:
		var ratio: float = pl._fuel / pl.torch_duration
		var prox: float  = 1.0 - dist / TORCH_DIST
		target_speed = lerpf(target_speed, TORCH_SLOW, ratio * prox * 1.4)

	_velocity = _velocity.lerp(dir * target_speed, steer * delta)
	global_position += _velocity * delta

	if _velocity.length() > 8.0:
		scale.x = sign(_velocity.x)

func _build() -> void:
	var tex := _radial_tex()

	_aura = PointLight2D.new()
	_aura.position = Vector2(0, -30)
	_aura.color = Color(0.32, 0.10, 0.04)
	_aura.energy = 0.06
	_aura.texture_scale = 5.5
	_aura.texture = tex
	add_child(_aura)

	_body = Polygon2D.new()
	_body.color = Color(0.10, 0.07, 0.13)
	_body.polygon = _oval(0, 0, 75, 55)
	add_child(_body)

	var belly := Polygon2D.new()
	belly.color = Color(0.16, 0.11, 0.20)
	belly.polygon = _oval(0, 14, 42, 28)
	add_child(belly)

	var head := Polygon2D.new()
	head.color = Color(0.10, 0.07, 0.13)
	head.polygon = _oval(0, -70, 50, 42)
	add_child(head)

	for s in [-1, 1]:
		var ear := Polygon2D.new()
		ear.color = Color(0.10, 0.07, 0.13)
		ear.polygon = PackedVector2Array([
			Vector2(s * 9, -97), Vector2(s * 20, -130), Vector2(s * 43, -97)
		])
		add_child(ear)
		var inner := Polygon2D.new()
		inner.color = Color(0.42, 0.10, 0.18, 0.75)
		inner.polygon = PackedVector2Array([
			Vector2(s * 13, -100), Vector2(s * 20, -120), Vector2(s * 36, -100)
		])
		add_child(inner)

	_eye_l = Polygon2D.new()
	_eye_l.color = Color(0.90, 0.80, 0.28)
	_eye_l.polygon = _oval(-27, -68, 13, 9)
	add_child(_eye_l)
	_eye_r = Polygon2D.new()
	_eye_r.color = Color(0.90, 0.80, 0.28)
	_eye_r.polygon = _oval(27, -68, 13, 9)
	add_child(_eye_r)

	_pupil_l = Polygon2D.new()
	_pupil_l.color = Color(0.04, 0.02, 0.04)
	_pupil_l.polygon = PackedVector2Array([Vector2(-2,-8),Vector2(2,-8),Vector2(2,8),Vector2(-2,8)])
	_pupil_l.position = Vector2(-27, -68)
	add_child(_pupil_l)
	_pupil_r = Polygon2D.new()
	_pupil_r.color = Color(0.04, 0.02, 0.04)
	_pupil_r.polygon = PackedVector2Array([Vector2(-2,-8),Vector2(2,-8),Vector2(2,8),Vector2(-2,8)])
	_pupil_r.position = Vector2(27, -68)
	add_child(_pupil_r)

	_glow_l = _eye_light(Vector2(-27, -68), tex)
	_glow_r = _eye_light(Vector2( 27, -68), tex)

	var tr := Node2D.new()
	tr.position = Vector2(72, 5)
	add_child(tr)
	var prev: Node2D = tr
	for i in 6:
		var piv := Node2D.new()
		piv.position = Vector2(17, 0)
		prev.add_child(piv)
		_tail_pivs.append(piv)
		var seg := Polygon2D.new()
		var t := maxf(2.5, 10.5 - i * 1.5)
		seg.color = Color(0.10, 0.07, 0.13)
		seg.polygon = PackedVector2Array([
			Vector2(0,-t), Vector2(17,-t*0.55), Vector2(17,t*0.55), Vector2(0,t)
		])
		piv.add_child(seg)
		prev = piv

func _build_kill() -> void:
	var area := Area2D.new()
	var cs   := CollisionShape2D.new()
	var circ := CircleShape2D.new()
	circ.radius = KILL_R
	cs.shape = circ
	area.add_child(cs)
	area.body_entered.connect(func(b: Node2D) -> void:
		if b.name == "Player":
			PlayerDeath.trigger(get_tree()))
	add_child(area)

func _eye_light(pos: Vector2, tex: GradientTexture2D) -> PointLight2D:
	var l := PointLight2D.new()
	l.position = pos
	l.color = Color(0.92, 0.72, 0.18)
	l.energy = 0.25
	l.texture_scale = 0.7
	l.texture = tex
	add_child(l)
	return l

func _oval(cx: float, cy: float, rx: float, ry: float, n: int = 14) -> PackedVector2Array:
	var arr := PackedVector2Array()
	for i in n:
		var a := TAU * i / n
		arr.append(Vector2(cx + cos(a) * rx, cy + sin(a) * ry))
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
	t.width = 256
	t.height = 256
	return t
