@tool
extends Node2D

enum State { IDLE, LINEUP, CHARGE, STUNNED }

const DETECT_DIST  := 380.0
const LINEUP_TIME  := 0.65
const CHARGE_SPEED := 340.0
const CHARGE_TIME  := 0.75
const STUN_TIME    := 3.2
const IDLE_WAIT    := 1.0
const KILL_R       := 38.0

var _state      := State.IDLE
var _timer      := IDLE_WAIT
var _charge_dir := Vector2.ZERO
var _player: Node2D = null
var _float_t    := 0.0

# All visuals live in here so the bob is purely cosmetic —
# global_position stays clean and is only changed during CHARGE.
var _visual: Node2D

var _body:   Polygon2D
var _aura:   PointLight2D
var _glow_l: PointLight2D
var _glow_r: PointLight2D

func _ready() -> void:
	for child in get_children():
		child.free()
	_build()
	if Engine.is_editor_hint():
		return
	add_to_group("ghost")
	_player = get_tree().current_scene.get_node_or_null("Player")
	_build_kill()

func _process(delta: float) -> void:
	if Engine.is_editor_hint():
		return
	_float_t += delta * 1.1
	_tick(delta)
	_animate()

func _tick(delta: float) -> void:
	match _state:
		State.IDLE:
			_visual.position.y = sin(_float_t) * 9.0
			_timer -= delta
			if _timer <= 0.0 and _player != null:
				if global_position.distance_to(_player.global_position) < DETECT_DIST:
					_enter_lineup()

		State.LINEUP:
			_visual.position.y = sin(_float_t) * 4.0
			_timer -= delta
			if _timer <= 0.0:
				_state = State.CHARGE
				_timer = CHARGE_TIME

		State.CHARGE:
			_visual.position.y = 0.0
			global_position += _charge_dir * CHARGE_SPEED * delta
			_timer -= delta
			if _timer <= 0.0:
				_state = State.IDLE
				_timer = IDLE_WAIT

		State.STUNNED:
			_visual.position.y = sin(_float_t) * 5.0
			_timer -= delta
			if _timer <= 0.0:
				_state = State.IDLE
				_timer = IDLE_WAIT

func _enter_lineup() -> void:
	_state = State.LINEUP
	_timer = LINEUP_TIME
	var offset := _player.global_position - global_position
	if abs(offset.x) >= abs(offset.y):
		_charge_dir = Vector2(sign(offset.x), 0.0)
	else:
		_charge_dir = Vector2(0.0, sign(offset.y))

func stun() -> void:
	# Don't touch global_position — just switch state.
	# If mid-charge, stop here so the ghost doesn't keep sliding.
	if _state == State.CHARGE:
		_charge_dir = Vector2.ZERO
	_state = State.STUNNED
	_timer = STUN_TIME

func _animate() -> void:
	match _state:
		State.IDLE:
			_body.color    = Color(0.80, 0.87, 1.0, 0.70)
			_aura.energy   = 0.18
			_glow_l.energy = 0.30
			_glow_r.energy = 0.30

		State.LINEUP:
			var flash := sin(_float_t * 14.0) * 0.5 + 0.5
			_body.color    = Color(0.88, 0.94, 1.0, 0.80 + flash * 0.15)
			_aura.energy   = 0.30 + flash * 0.20
			_glow_l.energy = 0.70 + flash * 0.80
			_glow_r.energy = 0.70 + flash * 0.80

		State.CHARGE:
			_body.color    = Color(0.94, 0.97, 1.0, 1.0)
			_aura.energy   = 0.65
			_glow_l.energy = 2.2
			_glow_r.energy = 2.2

		State.STUNNED:
			var flicker := sin(_float_t * 22.0) * 0.5 + 0.5
			_body.color    = Color(1.0, 0.82, 0.30, 0.45 + flicker * 0.25)
			_aura.energy   = flicker * 0.12
			_glow_l.energy = flicker * 0.25
			_glow_r.energy = flicker * 0.25

func _build() -> void:
	var tex := _radial_tex()

	_visual = Node2D.new()
	add_child(_visual)

	_aura = PointLight2D.new()
	_aura.color = Color(0.55, 0.70, 1.0)
	_aura.energy = 0.18
	_aura.texture_scale = 4.0
	_aura.texture = tex
	_visual.add_child(_aura)

	for i in 4:
		var seg := Polygon2D.new()
		var w  := 18.0 - i * 3.5
		var yb := 35.0 + i * 20.0
		var yt := 20.0 + i * 20.0
		seg.color = Color(0.80, 0.87, 1.0, 0.55 - i * 0.12)
		seg.polygon = PackedVector2Array([
			Vector2(-w, yt), Vector2(w, yt), Vector2(w * 0.4, yb), Vector2(-w * 0.4, yb)
		])
		_visual.add_child(seg)

	_body = Polygon2D.new()
	_body.color = Color(0.80, 0.87, 1.0, 0.70)
	_body.polygon = _oval(0, 0, 32, 38)
	_visual.add_child(_body)

	var eye_l := Polygon2D.new()
	eye_l.color = Color(0.3, 0.55, 1.0)
	eye_l.polygon = _oval(-12, -10, 6, 8)
	_visual.add_child(eye_l)

	var eye_r := Polygon2D.new()
	eye_r.color = Color(0.3, 0.55, 1.0)
	eye_r.polygon = _oval(12, -10, 6, 8)
	_visual.add_child(eye_r)

	_glow_l = PointLight2D.new()
	_glow_l.position = Vector2(-12, -10)
	_glow_l.color = Color(0.4, 0.65, 1.0)
	_glow_l.energy = 0.30
	_glow_l.texture_scale = 0.55
	_glow_l.texture = tex
	_visual.add_child(_glow_l)

	_glow_r = PointLight2D.new()
	_glow_r.position = Vector2(12, -10)
	_glow_r.color = Color(0.4, 0.65, 1.0)
	_glow_r.energy = 0.30
	_glow_r.texture_scale = 0.55
	_glow_r.texture = tex
	_visual.add_child(_glow_r)

func _build_kill() -> void:
	var area := Area2D.new()
	var cs   := CollisionShape2D.new()
	var circ := CircleShape2D.new()
	circ.radius = KILL_R
	cs.shape = circ
	area.add_child(cs)
	area.body_entered.connect(func(b: Node2D) -> void:
		if b.name == "Player" and _state != State.STUNNED:
			PlayerDeath.trigger(get_tree()))
	add_child(area)

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
