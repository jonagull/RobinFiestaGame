extends Node2D

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color.BLACK)

func _exit_tree() -> void:
	RenderingServer.set_default_clear_color(Color(0.3, 0.3, 0.3))
