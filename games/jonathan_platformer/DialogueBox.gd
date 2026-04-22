extends Control

signal finished

const CHARS_PER_SEC := 40.0   # typewriter speed
const BAR_ANIM_TIME := 0.35   # seconds for bars to slide in/out

var _lines: Array[String] = []
var _active := false
var _can_advance := false
var _typing := false
var _skip_typing := false
var _current_line := 0
var _full_text := ""

@onready var top_bar: ColorRect     = $TopBar
@onready var bottom_panel: ColorRect = $BottomPanel
@onready var speaker_label: Label   = $BottomPanel/SpeakerName
@onready var dialogue_text: Label   = $BottomPanel/DialogueText
@onready var continue_hint: Label   = $BottomPanel/ContinueHint

func _ready() -> void:
	visible = false

func _unhandled_input(event: InputEvent) -> void:
	if not _active or not _can_advance:
		return
	if event is InputEventKey and event.keycode == KEY_E and event.pressed and not event.echo:
		if _typing:
			_skip_typing = true
		else:
			_advance()
		get_viewport().set_input_as_handled()

# ── Public ────────────────────────────────────────────────────────────────────

func start(lines: Array[String]) -> void:
	_lines = lines
	_current_line = 0
	_active = true
	_can_advance = false
	visible = true
	continue_hint.visible = false

	# Start bars off-screen
	top_bar.position.y    = -top_bar.size.y
	bottom_panel.position.y = get_viewport_rect().size.y

	var tween := create_tween().set_parallel()
	tween.tween_property(top_bar,     "position:y", 0.0,                                          BAR_ANIM_TIME).set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_QUART)
	tween.tween_property(bottom_panel,"position:y", get_viewport_rect().size.y - bottom_panel.size.y, BAR_ANIM_TIME).set_ease(Tween.EASE_OUT).set_trans(Tween.TRANS_QUART)
	await tween.finished
	_show_line()

# ── Private ───────────────────────────────────────────────────────────────────

func _show_line() -> void:
	_can_advance = false
	var line: String = _lines[_current_line]

	# Split "Speaker: text" into name + body
	var colon: int = line.find(": ")
	if colon != -1:
		speaker_label.text = line.substr(0, colon)
		_full_text = line.substr(colon + 2)
	else:
		speaker_label.text = ""
		_full_text = line

	dialogue_text.text = ""
	continue_hint.visible = false
	_typing = true
	_skip_typing = false

	var char_count: int = _full_text.length()
	for i in char_count:
		if _skip_typing:
			dialogue_text.text = _full_text
			break
		dialogue_text.text = _full_text.substr(0, i + 1)
		await get_tree().create_timer(1.0 / CHARS_PER_SEC).timeout

	_typing = false
	dialogue_text.text = _full_text
	continue_hint.visible = true
	_can_advance = true

func _advance() -> void:
	_current_line += 1
	if _current_line >= _lines.size():
		_close()
	else:
		_show_line()

func _close() -> void:
	_can_advance = false
	continue_hint.visible = false

	var tween := create_tween().set_parallel()
	tween.tween_property(top_bar,     "position:y", -top_bar.size.y,                BAR_ANIM_TIME).set_ease(Tween.EASE_IN).set_trans(Tween.TRANS_QUART)
	tween.tween_property(bottom_panel,"position:y", get_viewport_rect().size.y, BAR_ANIM_TIME).set_ease(Tween.EASE_IN).set_trans(Tween.TRANS_QUART)
	await tween.finished

	visible = false
	_active = false
	finished.emit()
