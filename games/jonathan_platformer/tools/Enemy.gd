extends AnimatableBody2D

@export var speed: float  = 120.0
@export var loop:  bool   = false

var _points: Array[Vector2] = []
var _index: int = 0
var _dir:   int = 1

func _ready() -> void:
	$AnimatedSprite2D.play("default")
	sync_to_physics = false
	for child in get_children():
		if child is Marker2D:
			_points.append(child.global_position)
	$HitZone.body_entered.connect(func(body: Node2D) -> void:
		if body.name == "Player":
			PlayerDeath.trigger(get_tree()))

func _physics_process(delta: float) -> void:
	if _points.size() < 2:
		return
	var target := _points[_index]
	var step   := speed * delta
	if global_position.distance_to(target) <= step:
		global_position = target
		if loop:
			_index = (_index + 1) % _points.size()
		else:
			_index += _dir
			if _index >= _points.size():
				_index = _points.size() - 2
				_dir   = -1
			elif _index < 0:
				_index = 1
				_dir   = 1
	else:
		move_and_collide((target - global_position).normalized() * step)
