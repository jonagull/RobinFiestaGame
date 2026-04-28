extends StaticBody2D

@export var bounce_force: float = 1000.0

var _player: Node2D = null

func _ready() -> void:
	process_physics_priority = 1
	$BounceZone.body_entered.connect(func(body: Node2D) -> void:
		if body.name == "Player": _player = body)
	$BounceZone.body_exited.connect(func(body: Node2D) -> void:
		if body.name == "Player": _player = null)

func _physics_process(_delta: float) -> void:
	if _player == null:
		return
	var bounce_dir: Vector2 = (-global_transform.y).normalized()
	var vel: Vector2 = _player.get("velocity")
	if vel.dot(bounce_dir) <= 0:
		_player.set("velocity", bounce_dir * bounce_force)
