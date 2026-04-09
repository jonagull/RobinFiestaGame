# Robin Fiesta — Claude Code Guide

This is a Godot 4 C# project where each colleague builds a small game about Robin.
The launcher auto-discovers games, so you only ever work inside your own folder.

## Project layout

```
launcher/          ← launcher screen, don't touch
shared/            ← shared Robin assets and utility scripts
games/_template/   ← copy this to start your game
games/your_game/   ← your game lives here, entirely
```

## Starting your game

1. Copy `games/_template/` and rename it `games/yourname_gameidea/`
2. Update `game_info.json` with your title, name, and description
3. Rename the `main_scene` path in `game_info.json` to match your folder
4. Open Godot, your game will appear on the launcher automatically

## Rules Claude should follow

- **Only create or edit files inside `games/your_game/`** unless explicitly asked to touch shared files.
- **Never edit** `launcher/`, `project.godot`, or another colleague's game folder.
- **Don't change the autoload list** in `project.godot`.
- When adding assets, put them inside the game folder — not at the project root.
- Use `ReturnToLauncher()` (defined in `Game.cs`) to go back to the menu when the game ends.

## Godot 4 C# conventions used here

- Scenes use `.tscn` format (text), scripts use `.cs`
- Node access: prefer `GetNode<T>("NodeName")` or `%UniqueNodeName`
- Scene switching: `GetTree().ChangeSceneToFile("res://path/to/Scene.tscn")`
- Godot built-in types: `Vector2`, `GD.Print()`, `Input.IsActionPressed()`, etc.

## Saving a score

At the end of your game, save a result to `user://scores/yourname_gameidea.json`:

```csharp
private void SaveScore(int score)
{
    var data = new Godot.Collections.Dictionary
    {
        { "game", "yourname_gameidea" },  // match your folder name
        { "player", "Your Name" },
        { "score", score }
    };

    DirAccess.MakeDirRecursiveAbsolute("user://scores");
    using var file = FileAccess.Open("user://scores/yourname_gameidea.json", FileAccess.ModeFlags.Write);
    file.StoreString(Json.Stringify(data));
}
```

Call this before `ReturnToLauncher()`. Each run overwrites the previous score — only the best counts toward the leaderboard.

## What Robin looks like

Robin is the main character. Any shared sprites or animations for Robin live in
`shared/characters/robin/`. Use these if you want a consistent Robin across games,
or draw your own version inside your game folder.

## Getting help

- Ask Claude to scaffold a scene, write a script, or explain a Godot API
- Keep requests scoped to your game folder
- If you need something in `shared/`, check with the project owner first
