@tool
extends Node2D

const STALK_SPEED   := 55.0
const CHARGE_SPEED  := 200.0
const CHARGE_DIST   := 250.0
const STEER_STALK   := 3.2
const STEER_CHARGE  := 0.7
const TORCH_DIST    := 300.0
const TORCH_SLOW    := 18.0
const KILL_R        := 58.0
@export var wake_delay: float = 5.5
@export var auto_wake: bool   = true
@export var shoot_interval: float = 2.5
@export var projectile_speed: float = 260.0

var _awake    := false
var _wake_t   := 0.0
var _player: Node2D = null
var _velocity := Vector2.ZERO

var _hp      := 10
var _stunned := false
var _shoot_t := 0.0

var _shield_active    := false
var _hits_vulnerable  := 0
var _pillars: Array   = []
var _active_pillars   := 0
var _shield_mesh: Polygon2D = null
var _shield_t         := 0.0

const PROJECTILE = preload("res://games/jonathan_platformer/tools/Projectile.tscn")

var _breath_t := 0.0
var _tail_t   := 0.0
var _eye_t    := 0.0

var _body:      Polygon2D
var _eye_l:     Polygon2D
var _eye_r:     Polygon2D
var _pupil_l:   Polygon2D
var _pupil_r:   Polygon2D
var _tail_pivs: Array = []
var _glow_l:    PointLight2D
var _glow_r:    PointLight2D
var _aura:      PointLight2D
var _kill_area: Area2D

# floating-eyes intro state
var _preview: Node2D    = null
var _prev_pl: Polygon2D = null
var _prev_pr: Polygon2D = null

func _ready() -> void:
	for child in get_children():
		child.free()
	_build()
	if Engine.is_editor_hint():
		return
	_wake_t = wake_delay
	_player = get_tree().current_scene.get_node_or_null("Player")
	_build_kill()
	add_to_group("hittable")
	add_to_group("boss_cat")
	if not auto_wake:
		modulate.a = 0.0
		_kill_area.monitoring = false

func _process(delta: float) -> void:
	if Engine.is_editor_hint():
		return
	if not _awake and auto_wake:
		_wake_t -= delta
		if _wake_t <= 0.0:
			_on_wake()
	# Preview pupils track player while intro is running
	if _preview != null and is_instance_valid(_preview) and _player != null:
		var d := (_player.global_position - global_position).normalized() * 4.5
		if _prev_pl != null:
			_prev_pl.position = Vector2(-27, -68) + d
		if _prev_pr != null:
			_prev_pr.position = Vector2(27, -68) + d
	_breath_t += delta * 0.55
	var tail_mult: float = 1.6 if _awake else 0.35
	_tail_t += delta * tail_mult
	_eye_t  += delta * 1.7
	_animate()
	if _awake and not _stunned:
		_chase(delta)
		_shoot_t -= delta
		if _shoot_t <= 0.0:
			_shoot_t = shoot_interval
			_fire()
	if _shield_mesh != null:
		_shield_t += delta * 2.2
		_shield_mesh.modulate = Color(
			0.7 + sin(_shield_t) * 0.15,
			0.1,
			1.0,
			0.35 + sin(_shield_t * 1.4) * 0.1
		)

func _on_wake() -> void:
	_awake = true
	_kill_area.monitoring = true
	var tw := create_tween()
	tw.tween_property(_aura, "energy", 0.55, 0.08)
	tw.tween_property(_aura, "energy", 0.06, 0.9)
	# Find and activate all pillars
	await get_tree().process_frame
	_pillars = get_tree().get_nodes_in_group("shield_pillar")
	_active_pillars = _pillars.size()
	for p in _pillars:
		p.pillar_broken.connect(_on_pillar_broken)
		p.activate(self)
	if _pillars.size() > 0:
		_raise_shield()

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
	var is_charging := spd > STALK_SPEED * 1.4
	var steer: float        = STEER_CHARGE if is_charging else STEER_STALK
	var rage: float         = 1.0 + float(10 - _hp) * 0.12
	var target_speed: float = (CHARGE_SPEED if dist < CHARGE_DIST else STALK_SPEED) * rage
	var pl := _player.get_node_or_null("PlayerLight")
	if pl != null and pl._has_torch and pl._fuel > 0.0 and dist < TORCH_DIST:
		var ratio: float = pl._fuel / pl.torch_duration
		var prox: float  = 1.0 - dist / TORCH_DIST
		target_speed = lerpf(target_speed, TORCH_SLOW, ratio * prox * 1.4)
	_velocity = _velocity.lerp(dir * target_speed, steer * delta)
	global_position += _velocity * delta
	if _velocity.length() > 8.0:
		scale.x = sign(_velocity.x)

# ── Boss intro ────────────────────────────────────────────────────────────────

func pre_wake() -> void:
	_stunned = true
	_spawn_preview_eyes()
	# After 3.8s: destroy the floating eyes and fade the full cat in
	get_tree().create_timer(3.8).timeout.connect(func() -> void:
		if not is_instance_valid(self):
			return
		if _preview != null and is_instance_valid(_preview):
			_preview.queue_free()
			_preview = null
		var tw := create_tween()
		tw.set_trans(Tween.TRANS_SINE)
		tw.tween_property(self, "modulate:a", 1.0, 0.7)
		tw.tween_callback(func() -> void:
			if is_instance_valid(self):
				_stunned = false
				_on_wake()
		)
	)

func _spawn_preview_eyes() -> void:
	_preview = Node2D.new()
	_preview.global_position = global_position
	get_parent().add_child(_preview)

	var tex := _radial_tex()

	var gl := PointLight2D.new()
	gl.position    = Vector2(-27, -68)
	gl.color       = Color(0.92, 0.72, 0.18)
	gl.energy      = 0.0
	gl.texture_scale = 0.7
	gl.texture     = tex
	_preview.add_child(gl)

	var gr := PointLight2D.new()
	gr.position    = Vector2(27, -68)
	gr.color       = Color(0.92, 0.72, 0.18)
	gr.energy      = 0.0
	gr.texture_scale = 0.7
	gr.texture     = tex
	_preview.add_child(gr)

	var el := Polygon2D.new()
	el.color      = Color(0.90, 0.80, 0.28)
	el.polygon    = _oval(-27, -68, 13, 9)
	el.modulate.a = 0.0
	_preview.add_child(el)

	var er := Polygon2D.new()
	er.color      = Color(0.90, 0.80, 0.28)
	er.polygon    = _oval(27, -68, 13, 9)
	er.modulate.a = 0.0
	_preview.add_child(er)

	_prev_pl = Polygon2D.new()
	_prev_pl.color      = Color(0.04, 0.02, 0.04)
	_prev_pl.polygon    = PackedVector2Array([Vector2(-2,-8),Vector2(2,-8),Vector2(2,8),Vector2(-2,8)])
	_prev_pl.position   = Vector2(-27, -68)
	_prev_pl.modulate.a = 0.0
	_preview.add_child(_prev_pl)

	_prev_pr = Polygon2D.new()
	_prev_pr.color      = Color(0.04, 0.02, 0.04)
	_prev_pr.polygon    = PackedVector2Array([Vector2(-2,-8),Vector2(2,-8),Vector2(2,8),Vector2(-2,8)])
	_prev_pr.position   = Vector2(27, -68)
	_prev_pr.modulate.a = 0.0
	_preview.add_child(_prev_pr)

	_animate_preview(gl, gr, el, er)

func _animate_preview(gl: PointLight2D, gr: PointLight2D, el: Polygon2D, er: Polygon2D) -> void:
	# Phase 1 (0-1.1s): eyes slowly rise out of the darkness
	var t1 := create_tween().set_parallel(true)
	t1.tween_property(gl, "energy",     1.6, 1.1)
	t1.tween_property(gr, "energy",     1.6, 1.1)
	t1.tween_property(el, "modulate:a", 1.0, 1.1)
	t1.tween_property(er, "modulate:a", 1.0, 1.1)
	t1.tween_property(_prev_pl, "modulate:a", 1.0, 1.1)
	t1.tween_property(_prev_pr, "modulate:a", 1.0, 1.1)

	# Blink 1 (t=1.5s) — quick
	get_tree().create_timer(1.5).timeout.connect(func() -> void:
		if not is_instance_valid(el): return
		var bt := create_tween()
		bt.tween_property(el, "scale:y", 0.05, 0.07)
		bt.parallel().tween_property(er, "scale:y", 0.05, 0.07)
		bt.tween_property(el, "scale:y", 1.0,  0.12)
		bt.parallel().tween_property(er, "scale:y", 1.0,  0.12)
	)

	# Blink 2 (t=2.3s) — slower and heavier
	get_tree().create_timer(2.3).timeout.connect(func() -> void:
		if not is_instance_valid(el): return
		var bt := create_tween()
		bt.tween_property(el, "scale:y", 0.05, 0.12)
		bt.parallel().tween_property(er, "scale:y", 0.05, 0.12)
		bt.tween_property(el, "scale:y", 1.0,  0.18)
		bt.parallel().tween_property(er, "scale:y", 1.0,  0.18)
	)

	# Phase 3 (t=3.1s): eyes widen and burn bright just before the cat materialises
	get_tree().create_timer(3.1).timeout.connect(func() -> void:
		if not is_instance_valid(gl): return
		var wt := create_tween().set_parallel(true)
		wt.tween_property(gl, "energy",       4.0, 0.5)
		wt.tween_property(gr, "energy",       4.0, 0.5)
		wt.tween_property(gl, "texture_scale", 1.6, 0.5)
		wt.tween_property(gr, "texture_scale", 1.6, 0.5)
	)

# ── Damage ────────────────────────────────────────────────────────────────────

func take_hit(from_pos: Vector2) -> void:
	if _hp <= 0 or _stunned:
		return
	if _shield_active:
		# Shield flash — deflect hit
		var tw := create_tween()
		tw.tween_property(_shield_mesh, "modulate:a", 0.9, 0.05)
		tw.tween_property(_shield_mesh, "modulate:a", 0.35, 0.15)
		return
	_hp -= 1
	_hits_vulnerable += 1
	_stunned = true
	_velocity = (global_position - from_pos).normalized() * 340.0
	modulate = Color(1.0, 0.28, 0.28)
	get_tree().create_timer(0.14).timeout.connect(func() -> void:
		if is_instance_valid(self):
			modulate = Color.WHITE
	)
	if _hp <= 0:
		_die()
		return
	if _hits_vulnerable >= 2:
		# Idle then raise shield again
		get_tree().create_timer(2.0).timeout.connect(func() -> void:
			if is_instance_valid(self):
				_stunned = false
				_regen_shield()
		)
	else:
		get_tree().create_timer(0.6).timeout.connect(func() -> void:
			if is_instance_valid(self):
				_stunned = false
		)

func _die() -> void:
	var tween := create_tween()
	tween.tween_property(self, "modulate:a", 0.0, 0.9)
	tween.tween_callback(queue_free)

# ── Shield ────────────────────────────────────────────────────────────────────

func _raise_shield() -> void:
	_shield_active = true
	if _shield_mesh != null:
		_shield_mesh.visible = true

func _drop_shield() -> void:
	_shield_active = false
	_hits_vulnerable = 0
	if _shield_mesh != null:
		var tw := create_tween()
		tw.tween_property(_shield_mesh, "modulate:a", 0.0, 0.4)
		tw.tween_callback(func() -> void:
			if is_instance_valid(_shield_mesh):
				_shield_mesh.visible = false
				_shield_mesh.modulate.a = 0.35
		)

func _on_pillar_broken() -> void:
	_active_pillars -= 1
	if _active_pillars <= 0:
		_drop_shield()

func _regen_shield() -> void:
	# Pick a random pillar, reactivate it
	_pillars.shuffle()
	var picked: Node2D = null
	for p in _pillars:
		if is_instance_valid(p) and not p._active:
			picked = p
			break
	if picked == null:
		return  # all still active somehow
	picked.add_to_group("hittable")
	picked.activate(self)
	_active_pillars = 1
	_raise_shield()

# ── Shooting ──────────────────────────────────────────────────────────────────

func _fire() -> void:
	if _player == null:
		return
	var dir := (_player.global_position - global_position).normalized()
	var p := PROJECTILE.instantiate()
	get_parent().add_child(p)
	p.global_position = global_position + Vector2(0, -40)
	p.velocity = dir * projectile_speed

# ── Build ─────────────────────────────────────────────────────────────────────

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

	var name_label := Label.new()
	name_label.text = "Bakke"
	name_label.add_theme_font_size_override("font_size", 13)
	name_label.add_theme_color_override("font_color", Color(0.75, 0.6, 0.85))
	name_label.position = Vector2(-20, 4)
	add_child(name_label)

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

	_shield_mesh = Polygon2D.new()
	_shield_mesh.color   = Color(0.7, 0.1, 1.0, 0.35)
	_shield_mesh.polygon = _oval(0, -30, 105, 95)
	_shield_mesh.visible = false
	add_child(_shield_mesh)

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
	_kill_area = Area2D.new()
	var cs   := CollisionShape2D.new()
	var circ := CircleShape2D.new()
	circ.radius = KILL_R
	cs.shape = circ
	_kill_area.add_child(cs)
	_kill_area.body_entered.connect(func(b: Node2D) -> void:
		if b.name == "Player":
			PlayerDeath.trigger(get_tree()))
	add_child(_kill_area)

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
