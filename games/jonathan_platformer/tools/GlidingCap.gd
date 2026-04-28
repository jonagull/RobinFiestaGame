extends Node

const MAX_GLIDE_FALL := 65.0

var _player: CharacterBody2D
var _cap_root: Node2D
var _cap_dome: Polygon2D
var _cap_brim: Polygon2D

func _ready() -> void:
	process_physics_priority = 1
	_player = get_parent()
	_build_cap_visual()

func _physics_process(_delta: float) -> void:
	if not GameData.has_golf_cap:
		_cap_root.visible = false
		return

	_cap_root.visible = true
	var facing := -1.0 if _player.get("flip_h") else 1.0
	var gliding := Input.is_action_pressed("throw") \
		and GameData.selected_slot == 1 \
		and _player.velocity.y > 20.0

	if gliding:
		_player.velocity.y = minf(_player.velocity.y, MAX_GLIDE_FALL)
		_cap_root.position = Vector2(facing * 28, -32)
		_cap_root.scale    = Vector2(facing, 1.0)
		_cap_root.rotation = 0.5
	else:
		_cap_root.position = Vector2(0, -66)
		_cap_root.scale    = Vector2(facing, 1.0)
		_cap_root.rotation = 0.0

func _build_cap_visual() -> void:
	_cap_root = Node2D.new()
	_cap_root.visible = false

	# Dome
	_cap_dome = Polygon2D.new()
	_cap_dome.color = Color(0.9, 0.75, 0.15)
	_cap_dome.polygon = PackedVector2Array([
		Vector2(-11, 0), Vector2(11, 0),
		Vector2(9, -10), Vector2(-9, -10)
	])
	_cap_root.add_child(_cap_dome)

	# Brim
	_cap_brim = Polygon2D.new()
	_cap_brim.color = Color(0.75, 0.6, 0.1)
	_cap_brim.polygon = PackedVector2Array([
		Vector2(-10, 0), Vector2(16, 0),
		Vector2(16, 4),  Vector2(-10, 4)
	])
	_cap_root.add_child(_cap_brim)

	_player.get_node("Shape").add_child(_cap_root)
