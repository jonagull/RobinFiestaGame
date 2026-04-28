extends Area2D

const WAKE_DELAY := 2.5
const BASE_SPEED := 80.0
const MAX_SPEED := 260.0
const ACCEL_TIME := 25.0  # seconds to ramp from base to max speed

var _active := false
var _player: Node2D = null
var _game_over := false
var _elapsed := 0.0

func _ready() -> void:
	_player = get_node("../Player")
	body_entered.connect(_on_body_entered)
	# Call activate() from elsewhere to start the hunter

func activate() -> void:
	await get_tree().create_timer(WAKE_DELAY).timeout
	_active = true

func _physics_process(delta: float) -> void:
	if not _active or _game_over or _player == null:
		return
	_elapsed += delta
	var speed := lerpf(BASE_SPEED, MAX_SPEED, minf(_elapsed / ACCEL_TIME, 1.0))
	var dir := (_player.global_position - global_position).normalized()
	global_position += dir * speed * delta

func _on_body_entered(body: Node2D) -> void:
	if _game_over or body.name != "Player":
		return
	_game_over = true
	_active = false
	PlayerDeath.trigger(get_tree())
