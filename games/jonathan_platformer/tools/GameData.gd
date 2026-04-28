class_name GameData

## Persists spawn routing across scene changes.
static var spawn_tag: String = ""

## Lives — reset to 3 on game over so a fresh restart starts full.
static var lives: int = 3

## Last safe position to respawn at within a scene.
static var checkpoint_position: Vector2 = Vector2.ZERO

## Total deaths across the whole run — never resets.
static var deaths: int = 0

## Collectible items
static var has_golf_cap: bool = false
static var has_golf_club: bool = false

## Currently selected inventory slot (0 = golf ball, 1 = golf cap)
static var selected_slot: int = 0
