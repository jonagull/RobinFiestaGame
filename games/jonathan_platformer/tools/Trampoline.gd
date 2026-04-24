extends Area2D

@export var bounce_force: float = 1000.0

func _ready() -> void:
	body_entered.connect(_on_body_entered)

func _on_body_entered(body: Node2D) -> void:
	if body.name != "Player":
		return
	var vel: Vector2 = body.get("velocity")
	if vel.y > 0:  # only bounce when falling onto it
		body.set("velocity", Vector2(vel.x, -bounce_force))
		_squash_anim()

func _squash_anim() -> void:
	var tween := create_tween()
	tween.tween_property($Mat, "scale", Vector2(1.2, 0.5), 0.08)
	tween.tween_property($Mat, "scale", Vector2(0.9, 1.2), 0.1)
	tween.tween_property($Mat, "scale", Vector2.ONE,       0.12)
