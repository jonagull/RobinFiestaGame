extends VBoxContainer

# Each entry maps action names to a label child name.
# When any listed action is first pressed, that label is hidden.
const HINTS: Array = [
	{ "actions": ["left", "right"], "label": "MoveHint" },
	{ "actions": ["jump"],          "label": "JumpHint" },
	{ "actions": ["dash"],          "label": "DashHint" },
]

func _process(_delta: float) -> void:
	for hint in HINTS:
		var label: Label = get_node_or_null(hint["label"])
		if label == null or not label.visible:
			continue
		for action: String in hint["actions"]:
			if Input.is_action_just_pressed(action):
				label.visible = false
				break
