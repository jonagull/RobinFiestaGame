using Godot;

public partial class TapRobin : Control
{
	private int _score = 0;
	private float _timeLeft = 15.0f;
	private bool _gameOver = false;

	private Label _scoreLabel;
	private Label _timerLabel;
	private Button _robinButton;
	private Label _resultLabel;
	private Button _backButton;

	// Must match custom_minimum_size set on RobinButton in the scene
	private const float ButtonW = 120f;
	private const float ButtonH = 64f;

	public override void _Ready()
	{
		_scoreLabel  = GetNode<Label>("ScoreLabel");
		_timerLabel  = GetNode<Label>("TimerLabel");
		_robinButton = GetNode<Button>("RobinButton");
		_resultLabel = GetNode<Label>("ResultLabel");
		_backButton  = GetNode<Button>("BackButton");

		_robinButton.Pressed += OnRobinPressed;
		_backButton.Pressed  += ReturnToLauncher;

		_resultLabel.Visible = false;
		_backButton.Visible  = false;

		MoveRobin();
	}

	public override void _Process(double delta)
	{
		if (_gameOver) return;

		_timeLeft -= (float)delta;
		_timerLabel.Text = $"Time: {Mathf.CeilToInt(Mathf.Max(_timeLeft, 0))}";

		if (_timeLeft <= 0f)
			EndGame();
	}

	private void OnRobinPressed()
	{
		if (_gameOver) return;
		_score++;
		_scoreLabel.Text = $"Score: {_score}";
		MoveRobin();
	}

	private void MoveRobin()
	{
		var size = GetViewport().GetVisibleRect().Size;
		float x = (float)GD.RandRange(0, size.X - ButtonW);
		float y = (float)GD.RandRange(0, size.Y - ButtonH);
		_robinButton.Position = new Vector2(x, y);
	}

	private void EndGame()
	{
		_gameOver = true;
		_robinButton.Visible = false;
		_resultLabel.Text    = $"Time's up!\nYou caught Robin {_score} times!";
		_resultLabel.Visible = true;
		_backButton.Visible  = true;
		SaveScore(_score);
	}

	private void SaveScore(int score)
	{
		var data = new Godot.Collections.Dictionary
		{
			{ "game",   "jonathan_tap_robin" },
			{ "player", "Jonathan" },
			{ "score",  score }
		};

		DirAccess.MakeDirRecursiveAbsolute("user://scores");
		using var file = FileAccess.Open("user://scores/jonathan_tap_robin.json", FileAccess.ModeFlags.Write);
		file.StoreString(Json.Stringify(data));
	}

	private void ReturnToLauncher()
	{
		GetTree().ChangeSceneToFile("res://launcher/Launcher.tscn");
	}
}
