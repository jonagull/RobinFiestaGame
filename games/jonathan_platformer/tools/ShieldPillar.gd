extends Node2D

signal pillar_broken

const HIT_COOLDOWN := 0.35

var _hp        := 3
var _active    := false
var _cat: Node2D = null
var _cooldown  := 0.0
var _pulse_t   := 0.0

@onready var _beam:    Line2D     = $Beam
@onready var _glow:    PointLight2D = $Glow
@onready var _crystal: Polygon2D  = $Crystal

func _ready() -> void:
	add_to_group("shield_pillar")
	add_to_group("hittable")
	_beam.visible  = false
	_glow.energy   = 0.0
	_glow.texture  = _radial_tex()

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

func _process(delta: float) -> void:
	_cooldown = maxf(_cooldown - delta, 0.0)
	if not _active or _cat == null or not is_instance_valid(_cat):
		return
	_pulse_t += delta * 3.0
	# Update beam endpoint to cat chest
	_beam.set_point_position(1, to_local(_cat.global_position + Vector2(0, -40)))
	# Pulse beam alpha
	var pulse := 0.55 + sin(_pulse_t) * 0.35
	_beam.default_color = Color(0.7, 0.2, 1.0, pulse)
	_beam.width = 3.0 + sin(_pulse_t * 1.3) * 1.5
	# Pulse crystal glow
	_glow.energy = 1.2 + sin(_pulse_t) * 0.4

func activate(cat: Node2D) -> void:
	_cat     = cat
	_hp      = 3
	_active  = true
	visible  = true
	modulate = Color.WHITE
	_beam.visible = true
	_glow.energy  = 1.2
	var tw := create_tween()
	tw.tween_property(_crystal, "modulate", Color(1.4, 0.4, 2.0), 0.4)

func take_hit(_from_pos: Vector2) -> void:
	if not _active or _cooldown > 0.0:
		return
	_cooldown = HIT_COOLDOWN
	_hp -= 1
	var tw := create_tween()
	tw.tween_property(self, "modulate", Color(2.5, 2.5, 2.5), 0.06)
	tw.tween_property(self, "modulate", Color.WHITE, 0.12)
	if _hp <= 0:
		_break()

func _break() -> void:
	_active       = false
	_beam.visible = false
	_glow.energy  = 0.0
	remove_from_group("hittable")
	var tw := create_tween()
	tw.tween_property(self, "modulate:a", 0.0, 0.6)
	tw.tween_callback(func() -> void:
		visible      = false
		modulate     = Color.WHITE
	)
	pillar_broken.emit()
