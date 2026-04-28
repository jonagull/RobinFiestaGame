extends Area2D

@export_multiline var sign_text: String = "Write your sign text here."
@export var title: String = ""

var _player_nearby := false
var _open          := false

@onready var _prompt: Label   = $PromptLabel
@onready var _modal:  Control = $CanvasLayer/Modal

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	_modal.visible = false
	_prompt.visible = false
	$CanvasLayer/Modal/Title.text  = title
	$CanvasLayer/Modal/Title.visible = title != ""
	$CanvasLayer/Modal/Body.text   = sign_text

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("interact"):
		if _player_nearby and not _open:
			_open = true
			_modal.visible = true
			_prompt.visible = false
			get_viewport().set_input_as_handled()
		elif _open:
			_open = false
			_modal.visible = false
			_prompt.visible = _player_nearby
			get_viewport().set_input_as_handled()

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = true
		if not _open:
			_prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = false
		_prompt.visible = false
		_open = false
		_modal.visible = false
