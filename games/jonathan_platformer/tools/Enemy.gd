extends AnimatableBody2D

@export var speed: float  = 120.0
@export var loop:  bool   = false
@export var can_shoot: bool = false
@export var shoot_interval: float = 2.0
@export var projectile_speed: float = 280.0

var _points: Array[Vector2] = []
var _index: int = 0
var _dir:   int = 1
var _shoot_timer: float = 0.0

const PROJECTILE = preload("res://games/jonathan_platformer/tools/Projectile.tscn")

func _ready() -> void:
	$AnimatedSprite2D.play("default")
	sync_to_physics = false
	for child in get_children():
		if child is Marker2D:
			_points.append(child.global_position)
	$HitZone.body_entered.connect(func(body: Node2D) -> void:
		if body.name == "Player":
			PlayerDeath.trigger(get_tree()))
	_shoot_timer = shoot_interval

func _physics_process(delta: float) -> void:
	if can_shoot:
		_shoot_timer -= delta
		if _shoot_timer <= 0.0:
			_shoot_timer = shoot_interval
			_fire()

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

func _fire() -> void:
	var player := _find_player()
	if player == null:
		return
	var dir := (player.global_position - global_position).normalized()
	var p := PROJECTILE.instantiate()
	get_parent().add_child(p)
	p.global_position = global_position
	p.velocity = dir * projectile_speed

func _find_player() -> Node2D:
	return get_tree().current_scene.get_node_or_null("Player")
