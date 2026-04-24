class_name PlayerDeath

static func trigger(tree: SceneTree) -> void:
	GameData.deaths += 1
	var player := _find_player(tree.current_scene)
	if player == null:
		return
	player.set("velocity", Vector2.ZERO)
	player.global_position = GameData.checkpoint_position
	var sm := player.get_node_or_null("StateMachine")
	if sm:
		sm.activate_state_by_name("IdleState")

static func _find_player(node: Node) -> Node:
	if node.name == "Player":
		return node
	for child in node.get_children():
		var result := _find_player(child)
		if result != null:
			return result
	return null
