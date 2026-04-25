extends Node

@export var speed: float = 600.0

var _active    := false
var _player:    CharacterBody2D
var _collision: CollisionShape2D

func _ready() -> void:
	_player    = get_parent() as CharacterBody2D
	_collision = _player.get_node_or_null("CollisionShape2D")

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.physical_keycode == KEY_V and event.pressed and not event.echo:
		_active = !_active
		if _collision:
			_collision.set_deferred("disabled", _active)
		if not _active:
			_player.set("velocity", Vector2.ZERO)

func _process(delta: float) -> void:
	if not _active:
		return
	var dir := Input.get_vector("left", "right", "up", "down")
	_player.global_position += dir * speed * delta
	_player.set("velocity", Vector2.ZERO)
