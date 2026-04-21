@tool
extends StaticBody2D

@export var length: float = 200.0 : set = _set_length
@export var falls_right: bool = true : set = _set_falls_right
@export var stay_down: bool = false
@export var rotation_speed: float = 1.6

var _player_nearby := false
var _locked_down := false
var _prompt: Label = null

func _ready() -> void:
	_rebuild()
	if Engine.is_editor_hint():
		return
	_prompt = get_node_or_null("PromptLabel")
	rotation = _angle_up()
	$Zone.body_entered.connect(func(b): if b.name == "Player": _player_nearby = true; _update_prompt())
	$Zone.body_exited.connect(func(b):  if b.name == "Player": _player_nearby = false; _update_prompt())

func _physics_process(delta: float) -> void:
	if Engine.is_editor_hint():
		return
	var want_down := (_player_nearby and Input.is_key_pressed(KEY_E)) or _locked_down
	if want_down and not _locked_down and stay_down and _is_fully_down():
		_locked_down = true
		_update_prompt()
	var target := _angle_down() if want_down else _angle_up()
	rotation = move_toward(rotation, target, rotation_speed * delta)
	if _prompt and _prompt.visible:
		_prompt.global_position = global_position + Vector2(-30, -80)

func _update_prompt() -> void:
	if _prompt == null:
		return
	_prompt.visible = _player_nearby and not _locked_down

# ── Helpers ───────────────────────────────────────────────────────────────────

func _angle_up() -> float:
	return -PI / 2.0 if falls_right else PI / 2.0

func _angle_down() -> float:
	return 0.0

func _is_fully_down() -> bool:
	return absf(rotation - _angle_down()) < 0.05

# ── Rebuild visuals/collision when export vars change ─────────────────────────

func _set_length(value: float) -> void:
	length = value
	_rebuild()

func _set_falls_right(value: bool) -> void:
	falls_right = value
	_rebuild()

func _rebuild() -> void:
	if not is_node_ready():
		await ready

	var half_h := 8.0
	var col: CollisionShape2D = $CollisionShape2D
	var vis: Polygon2D        = $Visual
	var zone_col: CollisionShape2D = $Zone/CollisionShape2D

	# Bridge extends in +X when falls_right, -X when falls_left
	var x := length if falls_right else -length

	col.position.x  = x / 2.0
	col.position.y  = 0.0
	(col.shape as RectangleShape2D).size = Vector2(length, half_h * 2)

	vis.polygon = PackedVector2Array([
		Vector2(0, -half_h),
		Vector2(x, -half_h),
		Vector2(x,  half_h),
		Vector2(0,  half_h),
	])

	(zone_col.shape as CircleShape2D).radius = clampf(length * 0.5, 60.0, 120.0)
