extends Area2D

var _player_nearby := false
var _in_dialogue := false

@onready var prompt_label: Label   = $PromptLabel
@onready var dialogue_box: Control = get_node("../../UI/DialogueBox")

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	dialogue_box.finished.connect(_on_dialogue_finished)

func _unhandled_input(event: InputEvent) -> void:
	if not _player_nearby or _in_dialogue:
		return
	if event is InputEventKey and event.keycode == KEY_E and event.pressed and not event.echo:
		_in_dialogue = true
		prompt_label.visible = false
		dialogue_box.start()
		get_viewport().set_input_as_handled()

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = true
		prompt_label.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = false
		prompt_label.visible = false

func _on_dialogue_finished() -> void:
	_in_dialogue = false
	if _player_nearby:
		prompt_label.visible = true
