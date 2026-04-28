extends Node2D

@export var swing_action: String = "throw"

const SWING_FROM := -0.8   # club raised behind/up
const SWING_TO   :=  2.4   # club through follow-through
const SWING_TIME :=  0.22  # seconds for the arc

var _held       := false
var _swinging   := false
var _player:    Node2D = null
var _hit_ids:   Array  = []
var _swing_from := SWING_FROM
var _swing_to   := SWING_TO

@onready var _pickup_area:  Area2D           = $PickupArea
@onready var _pickup_shape: CollisionShape2D = $PickupArea/CollisionShape2D

func _ready() -> void:
	_pickup_area.body_entered.connect(_on_pickup)

func _process(_delta: float) -> void:
	if not _held or _player == null:
		return
	var selected := GameData.selected_slot == 2
	visible = selected
	if not selected:
		return
	var facing: float = -1.0 if _player.get("flip_h") else 1.0
	global_position = _player.global_position + Vector2(facing * 20.0, -28.0)
	scale.x = facing

	if not _swinging and Input.is_action_just_pressed(swing_action):
		_swing()

	if _swinging:
		_check_hits()

func _swing() -> void:
	_swinging = true
	_hit_ids.clear()
	var facing: float = -1.0 if _player.get("flip_h") else 1.0
	_swing_from = SWING_FROM * facing
	_swing_to   = SWING_TO   * facing
	rotation = _swing_from
	var tween := create_tween()
	tween.set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_IN)
	tween.tween_property(self, "rotation", _swing_to, SWING_TIME)
	tween.tween_callback(_end_swing)

func _end_swing() -> void:
	_swinging = false
	var tween := create_tween()
	tween.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	tween.tween_property(self, "rotation", _swing_from, 0.28)

func _check_hits() -> void:
	# to_global accounts for node position, rotation, and scale — correct for any facing
	var head: Vector2 = to_global(Vector2(0.0, -70.0))
	for target in get_tree().get_nodes_in_group("hittable"):
		var id := target.get_instance_id()
		if id in _hit_ids:
			continue
		if head.distance_to(target.global_position) < 100.0:
			_hit_ids.append(id)
			target.take_hit(global_position)

func _on_pickup(body: Node2D) -> void:
	if body.name != "Player" or _held:
		return
	_held = true
	_player = body
	_pickup_shape.set_deferred("disabled", true)
	GameData.has_golf_club = true
	GameData.selected_slot = 2
