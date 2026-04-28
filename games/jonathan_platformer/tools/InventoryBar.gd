extends HBoxContainer

const SLOT_SIZE := Vector2(36, 36)
const SLOT_GAP  := 4

var _slots: Array[Panel]  = []
var _hints: Array[Label]  = []
var _golf_ball: Node      = null

func _ready() -> void:
	add_theme_constant_override("separation", SLOT_GAP)

	# Slot 0 — golf ball (key 1 / default)
	var slot0 := _make_slot()
	add_child(slot0)
	_slots.append(slot0)
	_hints.append(_add_ball_icon(slot0))

	# Slot 1 — golf cap (key 2)
	var slot1 := _make_slot()
	add_child(slot1)
	_slots.append(slot1)
	_hints.append(_add_cap_icon(slot1))

	# Slot 2 — golf club (key 3)
	var slot2 := _make_slot()
	add_child(slot2)
	_slots.append(slot2)
	_hints.append(_add_club_icon(slot2))

	await get_tree().process_frame
	var found := get_tree().get_nodes_in_group("golf_ball")
	if found.size() > 0:
		_golf_ball = found[0]

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("next_slot"):
		GameData.selected_slot = (GameData.selected_slot + 1) % 3
		get_viewport().set_input_as_handled()
	elif event.is_action_pressed("slot_1"):
		GameData.selected_slot = 0
		get_viewport().set_input_as_handled()
	elif event.is_action_pressed("slot_2"):
		GameData.selected_slot = 1
		get_viewport().set_input_as_handled()
	elif event.is_action_pressed("slot_3"):
		GameData.selected_slot = 2
		get_viewport().set_input_as_handled()

func _process(_delta: float) -> void:
	_update_slot0()
	_update_slot1()
	_update_slot2()
	_update_selection_highlight()

func _update_slot0() -> void:
	# Golf ball in slot 0
	if _golf_ball == null:
		return
	var s: int = _golf_ball.get("state")
	if s == 2:   # UNAVAILABLE
		_slots[0].modulate = Color(1, 1, 1, 0.1)
		if _hints[0]: _hints[0].visible = false
	elif s == 0: # HELD
		_slots[0].modulate = Color(1, 1, 1, 1.0)
		if _hints[0]: _hints[0].visible = GameData.selected_slot == 0
	else:        # FLYING
		_slots[0].modulate = Color(1, 1, 1, 0.25)
		if _hints[0]: _hints[0].visible = false

func _update_slot1() -> void:
	# Golf cap in slot 1
	if not GameData.has_golf_cap:
		_slots[1].modulate = Color(1, 1, 1, 0.1)
		if _hints[1]: _hints[1].visible = false
	else:
		_slots[1].modulate = Color(1, 1, 1, 1.0)
		if _hints[1]: _hints[1].visible = GameData.selected_slot == 1

func _update_slot2() -> void:
	if not GameData.has_golf_club:
		_slots[2].modulate = Color(1, 1, 1, 0.1)
		if _hints[2]: _hints[2].visible = false
	else:
		_slots[2].modulate = Color(1, 1, 1, 1.0)
		if _hints[2]: _hints[2].visible = GameData.selected_slot == 2

func _update_selection_highlight() -> void:
	for i in _slots.size():
		var style := StyleBoxFlat.new()
		style.bg_color     = Color(0.08, 0.08, 0.08, 0.75)
		style.set_corner_radius_all(5)
		if i == GameData.selected_slot:
			style.border_color = Color(0.95, 0.85, 0.2, 1.0)
			style.set_border_width_all(3)
		else:
			style.border_color = Color(0.45, 0.45, 0.45, 0.9)
			style.set_border_width_all(2)
		_slots[i].add_theme_stylebox_override("panel", style)

func _make_slot() -> Panel:
	var slot := Panel.new()
	slot.custom_minimum_size = SLOT_SIZE
	var style := StyleBoxFlat.new()
	style.bg_color     = Color(0.08, 0.08, 0.08, 0.75)
	style.border_color = Color(0.45, 0.45, 0.45, 0.9)
	style.set_border_width_all(2)
	style.set_corner_radius_all(5)
	slot.add_theme_stylebox_override("panel", style)
	return slot

func _add_ball_icon(slot: Panel) -> Label:
	var icon := Polygon2D.new()
	var pts: PackedVector2Array
	for i in 8:
		var a := i * TAU / 8
		pts.append(Vector2(cos(a), sin(a)) * 9.0)
	icon.polygon  = pts
	icon.color    = Color(0.95, 0.95, 0.95)
	icon.position = SLOT_SIZE / 2
	slot.add_child(icon)
	return _add_key_hint(slot, "F")

func _add_cap_icon(slot: Panel) -> Label:
	var dome := Polygon2D.new()
	dome.color   = Color(0.15, 0.45, 0.9)
	dome.polygon = PackedVector2Array([
		Vector2(-9, -4), Vector2(9, -4),
		Vector2(7, -11), Vector2(-7, -11)
	])
	dome.position = SLOT_SIZE / 2
	slot.add_child(dome)

	var brim := Polygon2D.new()
	brim.color   = Color(0.1, 0.32, 0.7)
	brim.polygon = PackedVector2Array([
		Vector2(-9, -4), Vector2(13, -4),
		Vector2(13, -1), Vector2(-9, -1)
	])
	brim.position = SLOT_SIZE / 2
	slot.add_child(brim)

	return _add_key_hint(slot, "F")

func _add_club_icon(slot: Panel) -> Label:
	var shaft := Polygon2D.new()
	shaft.color   = Color(0.72, 0.72, 0.76)
	shaft.polygon = PackedVector2Array([Vector2(-1, -2), Vector2(1, -2), Vector2(1, 14), Vector2(-1, 14)])
	shaft.position = SLOT_SIZE / 2 + Vector2(-1, -10)
	slot.add_child(shaft)

	var head := Polygon2D.new()
	head.color   = Color(0.5, 0.5, 0.58)
	head.polygon = PackedVector2Array([Vector2(-1, 0), Vector2(9, 0), Vector2(9, 5), Vector2(-1, 5)])
	head.position = SLOT_SIZE / 2 + Vector2(-1, -12)
	slot.add_child(head)

	return _add_key_hint(slot, "F")

func _add_key_hint(slot: Panel, key: String) -> Label:
	var label := Label.new()
	label.text = key
	label.add_theme_font_size_override("font_size", 11)
	label.add_theme_color_override("font_color", Color(0.8, 0.8, 0.8, 0.9))
	label.set_anchors_and_offsets_preset(Control.PRESET_TOP_LEFT)
	label.position = Vector2(11, -16)
	slot.add_child(label)
	return label
