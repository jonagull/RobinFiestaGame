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
	var vel: Vector2 = _player.get("velocity")
	if vel.y >= 0:
		_player.set("velocity", Vector2(vel.x, -bounce_force))
