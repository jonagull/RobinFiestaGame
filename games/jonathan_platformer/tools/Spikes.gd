@tool
extends Area2D

@export var width: float = 64.0 : set = _set_width

func _ready() -> void:
	_rebuild()
	if Engine.is_editor_hint():
		return
	body_entered.connect(_on_body_entered)

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player":
		PlayerDeath.trigger(get_tree())

func _set_width(value: float) -> void:
	width = value
	_rebuild()

func _rebuild() -> void:
	if not is_node_ready():
		await ready
	var count := maxi(1, int(width / 16.0))
	var w_each := width / float(count)
	var pts := PackedVector2Array()
	for i in count:
		var x0 := -width * 0.5 + i * w_each
		pts.append(Vector2(x0,              0.0))
		pts.append(Vector2(x0 + w_each * 0.5, -20.0))
		pts.append(Vector2(x0 + w_each,     0.0))
	$Visual.polygon = pts
	($CollisionShape2D.shape as RectangleShape2D).size = Vector2(width, 16.0)
	$CollisionShape2D.position = Vector2(0.0, -8.0)
