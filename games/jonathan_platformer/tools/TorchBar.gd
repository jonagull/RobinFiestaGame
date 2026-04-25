extends Control

const BAR_W  := 12
const BAR_H  := 80
const PAD    := 16

var _ratio: float = 0.0
var _has_torch: bool = false

func _process(_delta: float) -> void:
	var player := _find_player()
	if player == null:
		return
	var light := player.get_node_or_null("PlayerLight")
	if light == null:
		return
	_has_torch = light._has_torch
	_ratio = clampf(light._fuel / light.torch_duration, 0.0, 1.0) if _has_torch else 0.0
	queue_redraw()

func _draw() -> void:
	if not _has_torch:
		return
	var x := size.x - PAD - BAR_W
	var y := size.y - PAD - BAR_H

	# background track
	draw_rect(Rect2(x, y, BAR_W, BAR_H), Color(0.15, 0.12, 0.1, 0.8))

	# fill — drains from top
	var fill_h := BAR_H * _ratio
	var fill_color := Color(1.0, lerpf(0.25, 0.65, _ratio), 0.05, 0.9)
	draw_rect(Rect2(x, y + BAR_H - fill_h, BAR_W, fill_h), fill_color)

	# flame tip on top when burning
	if _ratio > 0.0:
		var tip_x := x + BAR_W * 0.5
		var tip_y := y + BAR_H - fill_h
		var pts := PackedVector2Array([
			Vector2(tip_x - 5, tip_y),
			Vector2(tip_x, tip_y - 10),
			Vector2(tip_x + 5, tip_y),
		])
		draw_colored_polygon(pts, Color(1.0, 0.85, 0.3, 0.95))

func _find_player() -> Node:
	var scene := get_tree().current_scene
	return scene.get_node_or_null("Player")
