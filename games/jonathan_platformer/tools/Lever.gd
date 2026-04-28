extends Area2D

## Match this ID on any LeverPlatform you want this lever to control.
@export var lever_id: String = "lever_1"

var _player_nearby := false
var _holding       := false

@onready var _handle: Polygon2D = $Handle
@onready var _prompt: Label     = $PromptLabel

func _ready() -> void:
	add_to_group("lever_" + lever_id)
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	_prompt.visible = false

func _unhandled_input(event: InputEvent) -> void:
	if not _player_nearby:
		return
	if event.is_action_pressed("interact") and not _holding:
		_holding = true
		_prompt.text = "release"
		_handle.rotation = 0.5
	elif event.is_action_released("interact") and _holding:
		_holding = false
		_prompt.text = "hold"
		_handle.rotation = -0.5

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = true
		_prompt.text   = "hold" if not _holding else "release"
		_prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = false
		_prompt.visible = false
		# Release if player walks away while holding
		if _holding:
			_holding = false
			_handle.rotation = -0.5
