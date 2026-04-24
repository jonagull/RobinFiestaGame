extends AnimatableBody2D

## Must match the lever_id on the Lever you want to control this platform.
@export var lever_id: String    = "lever_1"
@export var drop_distance: float = 180.0
@export var drop_speed: float    = 90.0
@export var return_speed: float  = 40.0

var _origin: Vector2
var _lever: Node = null

func _ready() -> void:
	process_physics_priority = -1
	sync_to_physics = false
	_origin = global_position
	await get_tree().process_frame
	var levers := get_tree().get_nodes_in_group("lever_" + lever_id)
	if levers.size() > 0:
		_lever = levers[0]

func _physics_process(delta: float) -> void:
	if _lever == null:
		return
	var holding: bool = _lever.get("_holding")
	if holding and global_position.y < _origin.y + drop_distance:
		move_and_collide(Vector2(0, drop_speed * delta))
	elif not holding and global_position.y > _origin.y:
		move_and_collide(Vector2(0, -return_speed * delta))
