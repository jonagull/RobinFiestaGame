# Robin Fiesta

A collection of tiny Robin games, one per colleague. Pick a game from the launcher and play!

## How to add your game

1. **Copy** the `games/_template/` folder and rename it — use something like `games/yourname_gameidea/` (e.g. `games/alice_robin_chef/`)
2. **Edit `game_info.json`** inside your folder:
   ```json
   {
     "title": "Robin Becomes a Chef",
     "author": "Alice",
     "description": "Robin must cook 10 dishes before the dinner rush ends.",
     "main_scene": "res://games/alice_robin_chef/Game.tscn"
   }
   ```
3. **Build your game** inside your folder. All your scenes, scripts, and assets live there.
4. **Wire up the back button** — call `ReturnToLauncher()` when the player finishes/quits your game.

The launcher auto-discovers your game — no other files need to be touched.

## Folder structure

```
robin-fiesta/
├── launcher/          ← launcher screen (don't touch)
├── shared/
│   ├── characters/robin/   ← shared Robin sprites/assets
│   └── scripts/            ← utility scripts anyone can use
└── games/
    ├── _template/          ← copy this to start your game
    ├── alice_robin_chef/   ← Alice's game
    └── bob_robin_space/    ← Bob's game
```

## Rules

- **Stay in your folder.** Only edit files under `games/your_folder/`.
- **Shared assets** go in `shared/` — coordinate with others before changing them.
- **Don't edit** `launcher/` or `project.godot`.
- Name your folder `yourname_gameidea` to avoid collisions.
