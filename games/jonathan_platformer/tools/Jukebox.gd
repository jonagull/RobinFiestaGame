extends Node2D

@export var music: AudioStream = null
@export var volume_db: float = 0.0

var _nearby := false
var _playing := false

@onready var _player: AudioStreamPlayer = $AudioStreamPlayer
@onready var _prompt: Label = $Prompt

func _ready() -> void:
	add_to_group("jukebox")
	if music != null:
		_player.stream = music
	_player.volume_db = volume_db
	_prompt.visible = false
	$Body.body_entered.connect(_on_body_entered)
	$Body.body_exited.connect(_on_body_exited)

func _unhandled_input(event: InputEvent) -> void:
	if not _nearby:
		return
	if event.is_action_pressed("interact"):
		get_viewport().set_input_as_handled()
		_toggle()

func _toggle() -> void:
	if _playing:
		_player.stop()
		_playing = false
		_prompt.text = "[E] play"
	else:
		# Stop all other jukeboxes first
		for jb in get_tree().get_nodes_in_group("jukebox"):
			if jb != self:
				jb.stop_playback()
		_player.play()
		_playing = true
		_prompt.text = "[E] stop"

func stop_playback() -> void:
	if _playing:
		_player.stop()
		_playing = false
		if _nearby:
			_prompt.text = "[E] play"

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player":
		_nearby = true
		_prompt.text = "[E] play" if not _playing else "[E] stop"
		_prompt.visible = true

func _on_body_exited(body: Node2D) -> void:
	if body.name == "Player":
		_nearby = false
		_prompt.visible = false
