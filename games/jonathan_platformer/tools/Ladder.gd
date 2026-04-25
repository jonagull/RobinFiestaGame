extends Area2D

@export var climb_speed: float = 120.0

var _player: Node2D = null

func _ready() -> void:
	process_physics_priority = 1
	body_entered.connect(func(body: Node2D) -> void:
		if body.name == "Player": _player = body)
	body_exited.connect(func(body: Node2D) -> void:
		if body.name == "Player": _player = null)

func _physics_process(_delta: float) -> void:
	if _player == null:
		return
	if Input.is_action_pressed("jump"):
		return
	var up   := Input.is_action_pressed("up")
	var down := Input.is_action_pressed("down")
	if up or down:
		_player.set("velocity", Vector2(0, climb_speed * (1 if down else -1)))
	else:
		var vel: Vector2 = _player.get("velocity")
		_player.set("velocity", Vector2(vel.x, 0))
