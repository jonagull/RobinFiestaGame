extends Area2D

func _ready() -> void:
	body_entered.connect(_on_body_entered)

func _on_body_entered(body: Node2D) -> void:
	if body.name != "Player":
		return
	if GameData.lives < 3:
		GameData.lives += 1
		var mgr := get_tree().get_nodes_in_group("lives_manager")
		if mgr.size() > 0:
			mgr[0]._update_display()
	queue_free()
