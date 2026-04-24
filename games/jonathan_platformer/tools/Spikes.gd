extends Area2D

func _ready() -> void:
	body_entered.connect(func(body: Node2D) -> void:
		if body.name == "Player":
			PlayerDeath.trigger(get_tree()))
