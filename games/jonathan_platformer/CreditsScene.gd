extends Node2D

# ── Customise here ──────────────────────────────────────────────
@export var header_image: Texture2D = null   # drag a texture in the editor

@export var lines: Array[String] = [
	"",
	"Thanks for the hiring me boss!",
	"",
	"",
	"Good luck at Tidsbanken",
	"",
	"",
	"Robin survived",
	"(this time)",
	"",
	"",
	"Thanks for everything",
	"",
	"",
	"🎉",
]

@export var scroll_speed: float = 60.0   # pixels per second
# ────────────────────────────────────────────────────────────────

var _container: Control = null
var _scrolling := false

func _ready() -> void:
	RenderingServer.set_default_clear_color(Color(0.06, 0.05, 0.10))
	var vp := get_viewport().get_visible_rect().size
	_build_bg(vp)
	_build_credits(vp)
	_fade_in(vp)

func _exit_tree() -> void:
	RenderingServer.set_default_clear_color(Color(0.3, 0.3, 0.3))

func _build_bg(vp: Vector2) -> void:
	var canvas := CanvasLayer.new()
	canvas.layer = -10
	add_child(canvas)
	var bg := ColorRect.new()
	bg.color = Color(0.06, 0.05, 0.10)
	bg.size = vp
	canvas.add_child(bg)

func _build_credits(vp: Vector2) -> void:
	var canvas := CanvasLayer.new()
	canvas.layer = 10
	add_child(canvas)

	_container = Control.new()
	_container.position = Vector2(0, vp.y)   # starts just below screen
	canvas.add_child(_container)

	var cursor_y := 0.0
	var center_x := vp.x / 2.0

	# Optional header image with caption
	if header_image != null:
		var caption := Label.new()
		caption.text = "Nico and Robin live happily ever after at:"
		caption.add_theme_font_size_override("font_size", 20)
		caption.add_theme_color_override("font_color", Color(0.88, 0.84, 0.96))
		caption.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		caption.custom_minimum_size = Vector2(vp.x, 0)
		caption.position = Vector2(0, cursor_y)
		_container.add_child(caption)
		cursor_y += 40.0

		var img := TextureRect.new()
		img.texture = header_image
		img.expand_mode = TextureRect.EXPAND_FIT_WIDTH_PROPORTIONAL
		img.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
		img.custom_minimum_size = Vector2(200, 150)
		img.position = Vector2(center_x - 100, cursor_y)
		_container.add_child(img)
		cursor_y += 180.0

	# Text lines
	for line in lines:
		var lbl := Label.new()
		lbl.text = line
		lbl.add_theme_font_size_override("font_size", 22)
		lbl.add_theme_color_override("font_color", Color(0.88, 0.84, 0.96))
		lbl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		lbl.custom_minimum_size = Vector2(vp.x, 0)
		lbl.position = Vector2(0, cursor_y)
		_container.add_child(lbl)
		cursor_y += 44.0

	_container.custom_minimum_size = Vector2(vp.x, cursor_y)
	_scrolling = true

func _process(delta: float) -> void:
	if not _scrolling or _container == null:
		return
	_container.position.y -= scroll_speed * delta
	# Once all text has scrolled off the top, return to launcher
	var done_y := -((_container.custom_minimum_size.y) + 80)
	if _container.position.y < done_y:
		_scrolling = false
		_return()

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("interact") or event.is_action_pressed("jump"):
		_scrolling = false
		_return()

func _return() -> void:
	var scene := load("res://games/nmf_migrations/Game.cs")
	# Use the shared ReturnToLauncher path
	get_tree().change_scene_to_file("res://launcher/Launcher.tscn")

func _fade_in(vp: Vector2) -> void:
	var canvas := CanvasLayer.new()
	canvas.layer = 50
	add_child(canvas)
	var overlay := ColorRect.new()
	overlay.color = Color(0.0, 0.0, 0.0, 1.0)
	overlay.size = vp
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	canvas.add_child(overlay)
	var tw := canvas.create_tween()
	tw.tween_property(overlay, "color:a", 0.0, 1.5)
	tw.tween_callback(canvas.queue_free)
