extends PointLight2D

## Radius in pixels of what the player can see. Tweak this per scene.
@export var vision_radius: float = 120.0

func _ready() -> void:
	var gradient := Gradient.new()
	gradient.set_color(0, Color(1.0, 1.0, 1.0, 1.0))  # solid white center
	gradient.set_color(1, Color(1.0, 1.0, 1.0, 0.0))  # hard fade at edge
	gradient.set_offset(0, 0.0)
	gradient.set_offset(1, 1.0)
	var tex := GradientTexture2D.new()
	tex.gradient  = gradient
	tex.fill      = GradientTexture2D.FILL_RADIAL
	tex.fill_from = Vector2(0.5, 0.5)
	tex.fill_to   = Vector2(1.0, 0.5)
	tex.width     = 512
	tex.height    = 512
	texture       = tex
	texture_scale = vision_radius / 256.0
	energy        = 1.0
