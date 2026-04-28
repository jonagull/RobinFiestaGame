@tool
extends Area2D

signal activated
signal deactivated

@export var width:    float = 64.0 : set = _set_width
## When true the plate fires once and stays pressed permanently.
@export var one_shot: bool  = false

var _bodies_on := 0
var _pressed   := false
var _fired     := false

func _ready() -> void:
	_rebuild()
	if Engine.is_editor_hint():
		return
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)

func _on_body_entered(body: Node2D) -> void:
	if not (body.name == "Player" or body is RigidBody2D):
		return
	_bodies_on += 1
	if not _pressed and not (one_shot and _fired):
		_pressed = true
		_fired   = true
		$Visual.color = Color(0.9, 0.75, 0.1)
		activated.emit()

func _on_body_exited(body: Node2D) -> void:
	if not (body.name == "Player" or body is RigidBody2D):
		return
	_bodies_on = maxi(_bodies_on - 1, 0)
	if _pressed and _bodies_on == 0 and not (one_shot and _fired):
		_pressed = false
		$Visual.color = Color(0.55, 0.4, 0.12)
		deactivated.emit()

func _set_width(value: float) -> void:
	width = value
	_rebuild()

func _rebuild() -> void:
	if not is_node_ready():
		await ready
	var hw := width * 0.5
	$Visual.polygon = PackedVector2Array([
		Vector2(-hw,     0.0),
		Vector2( hw,     0.0),
		Vector2( hw + 4, -8.0),
		Vector2(-hw - 4, -8.0),
	])
	($CollisionShape2D.shape as RectangleShape2D).size = Vector2(width + 8.0, 8.0)
	$CollisionShape2D.position = Vector2(0.0, -4.0)
