extends AnimatableBody2D

@export var speed: float = 450.0
@export var throw_action: String = "throw"

enum State { HELD, FLYING, UNAVAILABLE }
var state    := State.UNAVAILABLE
var velocity := Vector2.ZERO
var _player: CharacterBody2D = null

func _ready() -> void:
	process_physics_priority = -1
	sync_to_physics = false
	add_to_group("golf_ball")
	visible = false
	$CollisionShape2D.set_deferred("disabled", true)
	var notifier := VisibleOnScreenNotifier2D.new()
	add_child(notifier)
	notifier.screen_exited.connect(_on_screen_exited)
	await get_tree().process_frame
	_player = _find_player(get_tree().current_scene)
	$PlayerZone.body_entered.connect(_on_player_entered)

func _unhandled_input(event: InputEvent) -> void:
	if state != State.HELD:
		return
	if event.is_action_pressed(throw_action):
		_throw()

func _physics_process(delta: float) -> void:
	if state != State.FLYING:
		return
	var collision := move_and_collide(velocity * delta)
	if collision:
		velocity = velocity.bounce(collision.get_normal())

func _throw() -> void:
	if _player == null:
		return
	var facing := -1.0 if _player.get("flip_h") else 1.0
	global_position = _player.global_position + Vector2(facing * 24, -20)
	velocity = Vector2(facing * speed, 0)
	$CollisionShape2D.set_deferred("disabled", false)
	$PlayerZone/CollisionShape2D.set_deferred("disabled", true)
	state   = State.FLYING
	visible = true
	await get_tree().create_timer(0.15).timeout
	$PlayerZone/CollisionShape2D.set_deferred("disabled", false)

func _on_player_entered(body: Node2D) -> void:
	if body.name != "Player" or state != State.FLYING:
		return
	# Player at side level → pickup. Player clearly above → let them land and ride.
	if body.global_position.y >= global_position.y - 8:
		_pickup()

func _on_screen_exited() -> void:
	if state == State.FLYING:
		_pickup()

func unlock() -> void:
	state = State.HELD

func _pickup() -> void:
	state    = State.HELD
	visible  = false
	velocity = Vector2.ZERO
	$CollisionShape2D.set_deferred("disabled", true)

func _find_player(node: Node) -> CharacterBody2D:
	if node.name == "Player":
		return node as CharacterBody2D
	for child in node.get_children():
		var result := _find_player(child)
		if result != null:
			return result
	return null
