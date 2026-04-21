using Godot;

/// <summary>
/// Entry point for your mini-game.
/// Rename this class to match your game folder name (e.g. RobinChef, RobinAstronaut).
/// </summary>
public partial class Platformer : Node
{
    private Button _backButton;

    public override void _Ready()
    {
        // Your game starts here!
        _backButton = GetNode<Button>("UI/BackButton");
        _backButton.Pressed += ReturnToLauncher;

        var gameOverScreen = GetNode<Control>("UI/GameOverScreen");
        gameOverScreen.AddToGroup("game_over");

        GetNode<Button>("UI/GameOverScreen/RestartButton").Pressed += RestartGame;
        GetNode<Button>("UI/GameOverScreen/MenuButton").Pressed += ReturnToLauncher;
    }

    private void RestartGame()
    {
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }

    /// <summary>
    /// Call this when the player is done (win, lose, quit button, etc.)
    /// to return to the launcher screen.
    /// </summary>
    protected void ReturnToLauncher()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://launcher/Launcher.tscn");
    }
}
