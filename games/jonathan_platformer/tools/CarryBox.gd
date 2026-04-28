extends RigidBody2D

const CARRY_OFFSET := Vector2(0, -52)

var _player_nearby := false
var _carried := false
var _carrier: CharacterBody2D = null

func _ready() -> void:
	freeze_mode = RigidBody2D.FREEZE_MODE_KINEMATIC
	$Zone.body_entered.connect(_on_zone_entered)
	$Zone.body_exited.connect(_on_zone_exited)

func _unhandled_input(event: InputEvent) -> void:
	if not event.is_action_pressed("interact"):
		return
	if _carried:
		_drop()
	elif _player_nearby:
		_pickup()

func _physics_process(_delta: float) -> void:
	if not _carried or _carrier == null:
		return
	global_position = _carrier.global_position + CARRY_OFFSET
	$PromptLabel.global_position = global_position + Vector2(-40, -48)

func _pickup() -> void:
	_carrier = _find_player_in_zone()
	if _carrier == null:
		return
	$CollisionShape2D.set_deferred("disabled", true)
	freeze = true
	_carried = true
	$PromptLabel.visible = false

func _drop() -> void:
	if _carrier != null:
		var facing := -1.0 if _carrier.get("flip_h") else 1.0
		global_position = _carrier.global_position + Vector2(facing * 40, 0)
	$CollisionShape2D.set_deferred("disabled", false)
	freeze = false
	_carried = false
	_carrier = null
	$PromptLabel.visible = _player_nearby

func _on_zone_entered(body: Node2D) -> void:
	if body.name != "Player":
		return
	_player_nearby = true
	if not _carried:
		$PromptLabel.visible = true

func _on_zone_exited(body: Node2D) -> void:
	if body.name != "Player":
		return
	_player_nearby = false
	$PromptLabel.visible = false
	if _carried:
		_drop()

func _find_player_in_zone() -> CharacterBody2D:
	for body in $Zone.get_overlapping_bodies():
		if body.name == "Player":
			return body
	return null
