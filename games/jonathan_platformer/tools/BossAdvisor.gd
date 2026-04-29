extends CanvasLayer

const W          := 268.0
const H          := 82.0
const SLIDE_TIME := 0.28
const SHOW_TIME  := 7.0

var _queue:    Array[String] = []
var _busy      := false
var _panel:    Control       = null
var _text_lbl: Label         = null

func _ready() -> void:
	add_to_group("boss_advisor")
	layer = 10
	_build()

func say(text: String) -> void:
	_queue.append(text)
	if not _busy:
		_show_next()

func _show_next() -> void:
	if _queue.is_empty():
		_busy = false
		return
	_busy = true
	_text_lbl.text = _queue.pop_front()

	var vp := get_viewport().get_visible_rect().size
	_panel.position = Vector2(-(W + 20.0), vp.y - H - 160.0)
	_panel.visible = true

	var tw := create_tween()
	tw.set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tw.tween_property(_panel, "position:x", 12.0, SLIDE_TIME)
	await tw.finished

	await get_tree().create_timer(SHOW_TIME).timeout

	var tw2 := create_tween()
	tw2.set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_IN)
	tw2.tween_property(_panel, "position:x", -(W + 20.0), SLIDE_TIME)
	await tw2.finished

	_panel.visible = false
	_show_next()

func _build() -> void:
	_panel = Control.new()
	_panel.size = Vector2(W, H)
	_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_panel.visible = false
	add_child(_panel)

	# Dark background
	var bg := ColorRect.new()
	bg.color = Color(0.09, 0.07, 0.13, 0.94)
	bg.size = Vector2(W, H)
	_panel.add_child(bg)

	# Portrait strip
	var port := ColorRect.new()
	port.color = Color(0.13, 0.10, 0.21, 1.0)
	port.size = Vector2(68.0, H)
	_panel.add_child(port)

	# NPC head
	var head := ColorRect.new()
	head.color = Color(0.82, 0.65, 0.50, 1.0)
	head.size = Vector2(20.0, 18.0)
	head.position = Vector2(24.0, 7.0)
	_panel.add_child(head)

	# NPC body — blue, matching NPC.tscn
	var body := ColorRect.new()
	body.color = Color(0.35, 0.55, 0.85, 1.0)
	body.size = Vector2(26.0, 40.0)
	body.position = Vector2(21.0, 27.0)
	_panel.add_child(body)

	# Purple accent divider
	var div := ColorRect.new()
	div.color = Color(0.55, 0.28, 0.92, 0.8)
	div.size = Vector2(2.0, H)
	div.position = Vector2(68.0, 0.0)
	_panel.add_child(div)

	# Speaker name
	var name_lbl := Label.new()
	name_lbl.text = "Nico"
	name_lbl.add_theme_font_size_override("font_size", 10)
	name_lbl.add_theme_color_override("font_color", Color(0.65, 0.45, 1.0))
	name_lbl.position = Vector2(76.0, 7.0)
	_panel.add_child(name_lbl)

	# Thin separator
	var sep := ColorRect.new()
	sep.color = Color(0.4, 0.25, 0.70, 0.38)
	sep.size = Vector2(W - 76.0, 1.0)
	sep.position = Vector2(76.0, 22.0)
	_panel.add_child(sep)

	# Speech text
	_text_lbl = Label.new()
	_text_lbl.add_theme_font_size_override("font_size", 11)
	_text_lbl.add_theme_color_override("font_color", Color(0.93, 0.91, 1.0))
	_text_lbl.position = Vector2(76.0, 26.0)
	_text_lbl.size = Vector2(W - 84.0, H - 30.0)
	_text_lbl.autowrap_mode = 3
	_panel.add_child(_text_lbl)
