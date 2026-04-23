extends Label

func _ready() -> void:
	_refresh()

func _process(_delta: float) -> void:
	_refresh()

func _refresh() -> void:
	text = "Deaths: %d" % GameData.deaths
