extends Area2D

const QUESTIONS := [
	"hey Robin\ncan you\nmigrate\nthis?",
	"fix this\nbug\nplease?",
	"review\nmy PR\n#142?",
	"deploy\nto\nprod?",
	"update\nthe\ndocs?",
	"write\ntests\npls?",
	"DB\nmigrate\nfailed!",
	"hotfix\nneeded!",
	"merge\nconflict\nhelp?",
	"rebase\nthis\nbranch?",
	"CI\nfailed\nagain",
	"URGENT!\nfix\nnow",
	"sprint\nplanning\n9am?",
	"standup\nin\n5min",
	"is this\nin\nscope?",
	"tech\ndebt\nticket?",
	"can you\njust\ncheck?",
	"prod\nis\ndown!",
	"who\nbroke\nstaging?",
	"need\nDB\naccess",
	"quick\ncall?\n5min",
	"rollback\nor\nfix?",
	"what's\nthe\nETA?",
	"new\nticket\nassigned!",
	"Robin\npls\nhelp",
	"blocked\non\nthis",
	"regression\nin\nprod",
	"can you\nown\nthis?",
]

var velocity := Vector2.ZERO

@onready var _visual: Polygon2D = $Visual

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	get_tree().create_timer(5.0).timeout.connect(queue_free)

	_visual.polygon = PackedVector2Array([-28, -44, 28, -44, 28, 44, -28, 44])
	_visual.color = Color(0.12, 0.08, 0.28, 0.93)

	var border := Polygon2D.new()
	border.polygon = PackedVector2Array([-28, -44, 28, -44, 28, 44, -28, 44])
	border.color = Color(0.55, 0.35, 1.0, 0.5)
	add_child(border)
	move_child(border, 0)

	var label := Label.new()
	label.text = QUESTIONS.pick_random()
	label.add_theme_font_size_override("font_size", 9)
	label.add_theme_color_override("font_color", Color(0.92, 0.88, 1.0))
	label.position = Vector2(-24, -42)
	add_child(label)

func _physics_process(delta: float) -> void:
	position += velocity * delta

func _on_body_entered(body: Node2D) -> void:
	if body.name == "Player":
		PlayerDeath.trigger(get_tree())
	queue_free()
