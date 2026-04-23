extends HBoxContainer

const SLOT_SIZE  := Vector2(36, 36)
const SLOT_GAP   := 4

var _slots: Array[Panel] = []
var _golf_ball: Node     = null
var _key_hint: Label     = null

func _ready() -> void:
	add_theme_constant_override("separation", SLOT_GAP)
	for i in 3:
		var slot := _make_slot()
		add_child(slot)
		_slots.append(slot)
		if i == 0:
			_key_hint = _add_ball_icon(slot)

	await get_tree().process_frame
	var found := get_tree().get_nodes_in_group("golf_ball")
	if found.size() > 0:
		_golf_ball = found[0]

func _process(_delta: float) -> void:
	if _golf_ball == null:
		return
	# State: HELD=0, FLYING=1, UNAVAILABLE=2
	var s: int = _golf_ball.get("state")
	if s == 2:   # UNAVAILABLE — slot nearly invisible, hide hint
		_slots[0].modulate = Color(1, 1, 1, 0.1)
		if _key_hint: _key_hint.visible = false
	elif s == 0: # HELD — full brightness, show hint
		_slots[0].modulate = Color(1, 1, 1, 1.0)
		if _key_hint: _key_hint.visible = true
	else:        # FLYING — dimmed, hide hint
		_slots[0].modulate = Color(1, 1, 1, 0.25)
		if _key_hint: _key_hint.visible = false

func _make_slot() -> Panel:
	var slot := Panel.new()
	slot.custom_minimum_size = SLOT_SIZE
	var style := StyleBoxFlat.new()
	style.bg_color        = Color(0.08, 0.08, 0.08, 0.75)
	style.border_color    = Color(0.45, 0.45, 0.45, 0.9)
	style.set_border_width_all(2)
	style.set_corner_radius_all(5)
	slot.add_theme_stylebox_override("panel", style)
	return slot

func _add_ball_icon(slot: Panel) -> Label:
	var icon := Polygon2D.new()
	var r := 9.0
	var pts: PackedVector2Array
	for i in 8:
		var a := i * TAU / 8
		pts.append(Vector2(cos(a), sin(a)) * r)
	icon.polygon  = pts
	icon.color    = Color(0.95, 0.95, 0.95)
	icon.position = SLOT_SIZE / 2
	slot.add_child(icon)

	# Key hint above the slot
	var key_label := Label.new()
	key_label.text = "F"
	key_label.add_theme_font_size_override("font_size", 11)
	key_label.add_theme_color_override("font_color", Color(0.8, 0.8, 0.8, 0.9))
	key_label.top_level = true
	key_label.position = slot.global_position + Vector2(13, -16)
	# Update position each frame since layout may shift
	slot.add_child(key_label)
	key_label.top_level = false
	key_label.set_anchors_and_offsets_preset(Control.PRESET_TOP_LEFT)
	key_label.position = Vector2(11, -16)
	return key_label
