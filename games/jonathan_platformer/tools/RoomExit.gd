@tool
extends Area2D

## Path to the scene file this exit leads to (click the folder icon to browse).
@export_file("*.tscn") var target_scene: String = ""
## Must match the tag on a SpawnPoint node in the target scene.
@export var spawn_tag: String = ""
@export var width:  float = 32.0 : set = _set_width
@export var height: float = 80.0 : set = _set_height

var _used := false

func _ready() -> void:
	_rebuild()
	if Engine.is_editor_hint():
		return
	body_entered.connect(_on_body_entered)

func _on_body_entered(body: Node2D) -> void:
	if _used or body.name != "Player" or target_scene.is_empty():
		return
	_used = true
	GameData.spawn_tag = spawn_tag
	get_tree().change_scene_to_file.call_deferred(target_scene)

func _set_width(v: float)  -> void: width  = v; _rebuild()
func _set_height(v: float) -> void: height = v; _rebuild()

func _rebuild() -> void:
	if not is_node_ready():
		await ready
	var hw := width  * 0.5
	$Visual.polygon = PackedVector2Array([
		Vector2(-hw, 0.0),    Vector2(hw, 0.0),
		Vector2( hw, -height), Vector2(-hw, -height),
	])
	($CollisionShape2D.shape as RectangleShape2D).size = Vector2(width, height)
	$CollisionShape2D.position = Vector2(0.0, -height * 0.5)
