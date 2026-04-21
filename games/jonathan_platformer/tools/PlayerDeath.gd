class_name PlayerDeath

## Call PlayerDeath.trigger(get_tree()) from any kill zone.
## Requires a node in the "game_over" group (GameOverScreen added from Platformer.cs _Ready).
static func trigger(tree: SceneTree) -> void:
	var nodes := tree.get_nodes_in_group("game_over")
	if nodes.size() > 0:
		nodes[0].visible = true
	tree.paused = true
