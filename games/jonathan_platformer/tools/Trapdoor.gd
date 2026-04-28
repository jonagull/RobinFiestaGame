@tool
extends StaticBody2D

@export var width: float = 120.0 : set = _set_width
@export var slide_distance: float = 140.0
@export var slide_duration: float = 0.6

var _opened := false

func open() -> void:
	if _opened:
		return
	_opened = true
	var tween := create_tween()
	tween.tween_property(self, "position:x", position.x - slide_distance, slide_duration) \
		.set_ease(Tween.EASE_IN_OUT).set_trans(Tween.TRANS_QUART)

func _set_width(value: float) -> void:
	width = value
	_rebuild()

func _rebuild() -> void:
	if not is_node_ready():
		await ready
	var hw := width * 0.5
	($CollisionShape2D.shape as RectangleShape2D).size = Vector2(width, 16)
	$Visual.polygon = PackedVector2Array([
		Vector2(-hw, -8), Vector2(hw, -8),
		Vector2( hw,  8), Vector2(-hw,  8),
	])
