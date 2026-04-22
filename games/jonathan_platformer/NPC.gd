extends Area2D

# Format each line as "Speaker: Text" — the speaker name is shown separately.
@export var lines: Array[String] = [
	"Stranger: Stop. Don't go any further.",
	"Stranger: I've seen others try. They all thought they could make it.",
	"Stranger: There's something beyond here. Something that hunts.",
	"Stranger: It doesn't run. It just... drifts toward you. Slow at first.",
	"Stranger: But it never stops. And it never loses you.",
	"Stranger: The moment you cross that point — it will know.",
	"Stranger: Turn back. Please.",
]

## Optional: drag a Drawbridge (or anything with an open() method) here.
## It will be opened automatically when this NPC's dialogue finishes.
@export var opens_on_finish: NodePath

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
		dialogue_box.start(lines)
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
	if opens_on_finish:
		get_node(opens_on_finish).open()
