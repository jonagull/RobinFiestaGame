extends Node2D

## Extra delay on top of the cascade's distance-based delay. Lets you hand-tune order.
@export var ignite_delay: float = 0.0

@onready var _flame: Polygon2D  = $Flame
@onready var _light: PointLight2D = $Light

var _lit := false

func _ready() -> void:
	add_to_group("chain_flame")
	_flame.visible = false
	_light.energy  = 0.0
	_build_light()

func ignite(cascade_delay: float) -> void:
	var total: float = cascade_delay + ignite_delay
	get_tree().create_timer(total).timeout.connect(_light_up)

func _light_up() -> void:
	if _lit:
		return
	_lit = true
	_flame.visible = true
	var tween := create_tween()
	tween.set_trans(Tween.TRANS_EXPO).set_ease(Tween.EASE_OUT)
	tween.tween_property(_light, "energy", 1.4, 0.35)

func _build_light() -> void:
	var gradient := Gradient.new()
	gradient.set_color(0, Color(1.0, 0.65, 0.2, 1.0))
	gradient.set_color(1, Color(1.0, 0.65, 0.2, 0.0))
	var tex := GradientTexture2D.new()
	tex.gradient  = gradient
	tex.fill      = GradientTexture2D.FILL_RADIAL
	tex.fill_from = Vector2(0.5, 0.5)
	tex.fill_to   = Vector2(1.0, 0.5)
	tex.width     = 256
	tex.height    = 256
	_light.texture       = tex
	_light.texture_scale = 2.0
