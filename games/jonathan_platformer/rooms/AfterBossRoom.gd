extends Node2D

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color(0.94, 0.92, 0.88))
	var vp := get_viewport().get_visible_rect().size
	_build_background(vp)
	_fade_in(vp)

func _exit_tree() -> void:
	RenderingServer.set_default_clear_color(Color(0.3, 0.3, 0.3))

func _build_background(vp: Vector2) -> void:
	var canvas := CanvasLayer.new()
	canvas.layer = -10
	add_child(canvas)
	var bg := ColorRect.new()
	bg.color = Color(0.835, 0.949, 0.95, 1.0)
	bg.size = vp
	canvas.add_child(bg)

func _fade_in(vp: Vector2) -> void:
	var canvas := CanvasLayer.new()
	canvas.layer = 50
	add_child(canvas)
	var overlay := ColorRect.new()
	overlay.color = Color(1.0, 1.0, 1.0, 1.0)
	overlay.size = vp
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	canvas.add_child(overlay)
	var tw := canvas.create_tween()
	tw.tween_property(overlay, "color:a", 0.0, 1.5)
	tw.tween_callback(canvas.queue_free)
