extends Node2D

func _ready() -> void:
	var vp := get_viewport().get_visible_rect().size
	_build_background(vp)
	_fade_in(vp)

func _build_background(vp: Vector2) -> void:
	var canvas := CanvasLayer.new()
	canvas.layer = -10
	add_child(canvas)
	var bg := ColorRect.new()
	bg.color = Color(0.91, 0.91, 0.95)
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
