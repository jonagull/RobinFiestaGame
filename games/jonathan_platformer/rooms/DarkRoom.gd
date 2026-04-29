extends Node2D

const SOLAR = preload("res://games/jonathan_platformer/assets/music/Solar.mp3")

var _music: AudioStreamPlayer = null

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color.BLACK)
	_music = AudioStreamPlayer.new()
	_music.stream = SOLAR
	_music.volume_db = 0.0
	_music.add_to_group("ambient_music")
	add_child(_music)
	_music.play()

func _exit_tree() -> void:
	RenderingServer.set_default_clear_color(Color(0.3, 0.3, 0.3))
	if _music and is_instance_valid(_music):
		_music.stop()
