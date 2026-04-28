@tool
extends Marker2D

## Must match the spawn_tag set on the RoomExit that leads here.
@export var tag: String = "default"

func _ready() -> void:
	if Engine.is_editor_hint():
		return
	if GameData.spawn_tag != tag:
		return
	var player := _find_player(get_tree().current_scene)
	if player != null:
		player.global_position = global_position
	GameData.spawn_tag = ""  # clear so it doesn't re-fire on scene reload

func _find_player(node: Node) -> Node:
	if node.name == "Player":
		return node
	for child in node.get_children():
		var result := _find_player(child)
		if result != null:
			return result
	return null
