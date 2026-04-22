class_name GameData

## Persists spawn routing across scene changes.
static var spawn_tag: String = ""

## Lives — reset to 3 on game over so a fresh restart starts full.
static var lives: int = 3

## Last safe position to respawn at within a scene.
static var checkpoint_position: Vector2 = Vector2.ZERO
