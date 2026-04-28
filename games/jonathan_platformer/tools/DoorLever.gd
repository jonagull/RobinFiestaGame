extends Area2D

## Must match the door_group on the BigDoor this lever controls.
@export var door_group: String = "big_door"

var _player_nearby := false
var _pulled        := false

@onready var _handle: Polygon2D = $Handle
@onready var _prompt: Label     = $PromptLabel

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	_prompt.visible = false

func _unhandled_input(event: InputEvent) -> void:
	if not _player_nearby or _pulled:
		return
	if event.is_action_pressed("interact"):
		_pull()
		get_viewport().set_input_as_handled()

func _pull() -> void:
	_pulled = true
	_prompt.visible = false
	var tween := create_tween()
	tween.set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tween.tween_property(_handle, "rotation", 0.65, 0.25)
	tween.parallel().tween_property(_handle, "color", Color(0.25, 0.25, 0.3), 0.25)
	for door in get_tree().get_nodes_in_group(door_group):
		door.on_lever_pulled()

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player" and not _pulled:
		_player_nearby = true
		_prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_player_nearby = false
		_prompt.visible = false
