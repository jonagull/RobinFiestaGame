extends HBoxContainer

const MAX_LIVES := 3

func _ready() -> void:
	add_to_group("lives_manager")
	# Capture initial player position as the first checkpoint
	var player := _find_player(get_tree().current_scene)
	if player:
		GameData.checkpoint_position = player.global_position
	# Hearts hidden — no lives system active
	for i in MAX_LIVES:
		get_child(i).visible = false

func take_damage() -> void:
	GameData.deaths += 1
	_respawn()

func _respawn() -> void:
	var player := _find_player(get_tree().current_scene)
	if player == null:
		return
	player.set("velocity", Vector2.ZERO)
	player.global_position = GameData.checkpoint_position

func _show_game_over() -> void:
	var nodes := get_tree().get_nodes_in_group("game_over")
	if nodes.size() > 0:
		nodes[0].visible = true
	get_tree().paused = true

func _update_display() -> void:
	for i in MAX_LIVES:
		var heart: Label = get_child(i)
		heart.modulate = Color(1, 0.2, 0.2) if i < GameData.lives else Color(0.3, 0.3, 0.3)

func _find_player(node: Node) -> Node:
	if node.name == "Player":
		return node
	for child in node.get_children():
		var result := _find_player(child)
		if result != null:
			return result
	return null
