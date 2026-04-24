extends Area2D

func _ready() -> void:
	body_entered.connect(_on_body_entered)

func _on_body_entered(body: Node2D) -> void:
	if body.name != "Player":
		return
	GameData.checkpoint_position = body.global_position
	queue_free()
