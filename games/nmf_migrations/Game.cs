using Godot;

/// <summary>
/// Entry point for your mini-game.
/// Rename this class to match your game folder name (e.g. RobinChef, RobinAstronaut).
/// </summary>
public partial class Game : Node
{
    public override void _Ready()
    {
        // Your game starts here!
    }

    /// <summary>
    /// Call this when the player is done (win, lose, quit button, etc.)
    /// to return to the launcher screen.
    /// </summary>
    protected void ReturnToLauncher()
    {
        GetTree().ChangeSceneToFile("res://launcher/Launcher.tscn");
    }
}
