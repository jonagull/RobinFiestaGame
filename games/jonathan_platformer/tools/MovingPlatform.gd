@tool
extends AnimatableBody2D

@export var width:     float   = 120.0         : set = _set_width
@export var distance:  float   = 150.0         : set = _set_distance
@export var direction: Vector2 = Vector2(1, 0) : set = _set_direction
@export var speed:     float   = 90.0

var _origin: Vector2
var _tween:  Tween

func _ready() -> void:
	_rebuild()
	if Engine.is_editor_hint():
		return
	_origin = position
	_start_moving()

func _start_moving() -> void:
	if speed <= 0.0 or distance <= 0.0:
		return
	var end      := _origin + direction.normalized() * distance
	var leg_time := distance / speed
	_tween = create_tween().set_loops()
	_tween.tween_property(self, "position", end,     leg_time).set_ease(Tween.EASE_IN_OUT)
	_tween.tween_property(self, "position", _origin, leg_time).set_ease(Tween.EASE_IN_OUT)

func _set_width(v: float)     -> void: width     = v; _rebuild()
func _set_distance(v: float)  -> void: distance  = v; _rebuild()
func _set_direction(v: Vector2) -> void: direction = v; _rebuild()

func _rebuild() -> void:
	if not is_node_ready():
		await ready
	var hw := width * 0.5
	$Visual.polygon = PackedVector2Array([
		Vector2(-hw, -8), Vector2(hw, -8),
		Vector2( hw,  8), Vector2(-hw,  8),
	])
	($CollisionShape2D.shape as RectangleShape2D).size = Vector2(width, 16.0)
