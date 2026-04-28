extends PointLight2D

@export var max_radius: float = 240.0
@export var min_radius: float = 30.0
@export var max_energy: float = 1.4
@export var min_energy: float = 0.08
@export var torch_duration: float = 18.0

var _fuel: float = 0.0
var _has_torch: bool = false
var _flare: float = 0.0
var _torch_visual: Node2D
var _torch_flame: Polygon2D

func _ready() -> void:
	_build_texture()
	_build_torch_visual()
	energy = 0.0

func _process(delta: float) -> void:
	_torch_visual.visible = _has_torch
	if not _has_torch:
		return
	if _fuel > 0.0:
		_fuel = maxf(_fuel - delta, 0.0)
		_flare = maxf(_flare - delta * 3.5, 0.0)
		var ratio := _fuel / torch_duration
		texture_scale = lerpf(min_radius, max_radius, ratio) / 256.0
		energy = lerpf(min_energy, max_energy, ratio) + _flare
		_torch_flame.visible = true
	else:
		energy = 0.0
		_torch_flame.visible = false

func give_torch() -> void:
	_has_torch = true

func light_torch() -> void:
	if not _has_torch:
		return
	_fuel = torch_duration
	_flare = 0.6
	energy = max_energy

func refuel() -> void:
	if not _has_torch:
		return
	_fuel = torch_duration
	_flare = 0.6
	energy = max_energy

func _build_torch_visual() -> void:
	_torch_visual = Node2D.new()
	_torch_visual.position = Vector2(24, -30)
	add_child(_torch_visual)

	var handle := Polygon2D.new()
	handle.color = Color(0.45, 0.3, 0.15)
	handle.polygon = PackedVector2Array([Vector2(-3, 0), Vector2(3, 0), Vector2(3, 20), Vector2(-3, 20)])
	_torch_visual.add_child(handle)

	var head := Polygon2D.new()
	head.color = Color(0.55, 0.45, 0.3)
	head.polygon = PackedVector2Array([Vector2(-6, -18), Vector2(6, -18), Vector2(4, 0), Vector2(-4, 0)])
	_torch_visual.add_child(head)

	_torch_flame = Polygon2D.new()
	_torch_flame.color = Color(1.0, 0.55, 0.1)
	_torch_flame.polygon = PackedVector2Array([Vector2(-4, -18), Vector2(0, -30), Vector2(4, -18)])
	_torch_flame.visible = false
	_torch_visual.add_child(_torch_flame)

	_torch_visual.visible = false

func _build_texture() -> void:
	var gradient := Gradient.new()
	gradient.set_color(0, Color(1.0, 1.0, 1.0, 1.0))
	gradient.set_color(1, Color(1.0, 1.0, 1.0, 0.0))
	gradient.set_offset(0, 0.0)
	gradient.set_offset(1, 1.0)
	var tex := GradientTexture2D.new()
	tex.gradient  = gradient
	tex.fill      = GradientTexture2D.FILL_RADIAL
	tex.fill_from = Vector2(0.5, 0.5)
	tex.fill_to   = Vector2(1.0, 0.5)
	tex.width     = 512
	tex.height    = 512
	texture       = tex
	texture_scale = max_radius / 256.0
