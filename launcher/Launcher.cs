using Godot;

/// <summary>
/// Auto-discovers all games in res://games/ by reading their game_info.json,
/// then presents them as buttons. No manual registration needed.
/// </summary>
public partial class Launcher : Control
{
	public override void _Ready()
	{
		GD.Print("Launcher _Ready");
		var list = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/GameList");
		GD.Print("Got list node: ", list);
		DiscoverGames(list);
	}

	private void DiscoverGames(VBoxContainer list)
	{
		var dirs = DirAccess.GetDirectoriesAt("res://games");
		GD.Print("Dirs found: ", dirs.Length);
		foreach (var dirName in dirs)
		{
			GD.Print("  Checking dir: ", dirName);
			// Skip template folders (anything starting with _)
			if (dirName.StartsWith("_")) continue;

			var infoPath = $"res://games/{dirName}/game_info.json";
			GD.Print("  Looking for: ", infoPath, " exists=", FileAccess.FileExists(infoPath));
			if (!FileAccess.FileExists(infoPath)) continue;

			using var file = FileAccess.Open(infoPath, FileAccess.ModeFlags.Read);
			var raw = file.GetAsText();

			var json = new Json();
			if (json.Parse(raw) != Error.Ok) continue;

			var info = json.Data.AsGodotDictionary();
			var title = info["title"].AsString();
			var author = info["author"].AsString();
			var description = info.ContainsKey("description") ? info["description"].AsString() : "";
			var mainScene = info["main_scene"].AsString();

			var button = new Button();
			button.Text = $"{title}  —  by {author}";
			button.TooltipText = description;
			button.CustomMinimumSize = new Vector2(0, 48);

			button.Pressed += () => GetTree().ChangeSceneToFile(mainScene);
			list.AddChild(button);
		}
	}
}
