class_name PlayerDeath

## Call PlayerDeath.trigger(get_tree()) from any kill zone.
static func trigger(tree: SceneTree) -> void:
	var mgr := tree.get_nodes_in_group("lives_manager")
	if mgr.size() > 0:
		mgr[0].take_damage()
