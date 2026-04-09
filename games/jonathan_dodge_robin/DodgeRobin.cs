using Godot;
using System.Collections.Generic;

public partial class DodgeRobin : Node2D
{
    private const float PlayerSpeed = 380f;
    private const float PlayerW     = 44f;
    private const float PlayerH     = 44f;
    private const float BlockW      = 52f;
    private const float BlockH      = 36f;

    private float _timeSurvived = 0f;
    private bool  _gameOver     = false;

    private Vector2 _playerPos;
    private readonly List<(Vector2 pos, float speed, float w)> _blocks = new();
    private float _spawnTimer = 0f;

    private Label  _timerLabel;
    private Label  _resultLabel;
    private Button _playAgainButton;
    private Button _backButton;

    private float _screenW;
    private float _screenH;

    // Difficulty helpers — ramp up aggressively over time
    private float BlockSpeed    => 240f + _timeSurvived * 14f;
    private float SpawnInterval => Mathf.Max(0.22f, 1.0f - _timeSurvived * 0.025f);
    // After 10s spawn 2 at once, after 20s spawn 3
    private int   SpawnCount    => 1 + (int)(_timeSurvived / 10f);

    public override void _Ready()
    {
        _timerLabel      = GetNode<Label>("UI/TimerLabel");
        _resultLabel     = GetNode<Label>("UI/ResultLabel");
        _playAgainButton = GetNode<Button>("UI/PlayAgainButton");
        _backButton      = GetNode<Button>("UI/BackButton");

        _playAgainButton.Pressed += Restart;
        _backButton.Pressed      += ReturnToLauncher;

        _resultLabel.Visible     = false;
        _playAgainButton.Visible = false;
        _backButton.Visible      = false;

        var rect = GetViewport().GetVisibleRect();
        _screenW = rect.Size.X;
        _screenH = rect.Size.Y;

        _playerPos = new Vector2(_screenW / 2f, _screenH - 60f);
    }

    public override void _Process(double delta)
    {
        if (_gameOver) return;

        float dt = (float)delta;
        _timeSurvived += dt;

        // Move player
        float dx = 0f;
        if (Input.IsActionPressed("ui_left"))  dx -= PlayerSpeed * dt;
        if (Input.IsActionPressed("ui_right")) dx += PlayerSpeed * dt;
        _playerPos.X = Mathf.Clamp(_playerPos.X + dx, PlayerW / 2f, _screenW - PlayerW / 2f);

        // Spawn blocks
        _spawnTimer -= dt;
        if (_spawnTimer <= 0f)
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                float x = (float)GD.RandRange(BlockW / 2f, _screenW - BlockW / 2f);
                // Vary block width slightly for chaos
                float w = (float)GD.RandRange(40.0, 70.0);
                // Stagger multi-spawns slightly so they don't all land at once
                float yOffset = i * -80f;
                _blocks.Add((new Vector2(x, -BlockH + yOffset), BlockSpeed, w));
            }
            _spawnTimer = SpawnInterval;
        }

        // Move blocks, check collision
        for (int i = _blocks.Count - 1; i >= 0; i--)
        {
            var (pos, speed, w) = _blocks[i];
            pos.Y += speed * dt;
            _blocks[i] = (pos, speed, w);

            if (pos.Y > _screenH + BlockH)
            {
                _blocks.RemoveAt(i);
                continue;
            }

            if (Mathf.Abs(pos.X - _playerPos.X) < (w / 2f + PlayerW / 2f) - 6f &&
                Mathf.Abs(pos.Y - _playerPos.Y) < (BlockH / 2f + PlayerH / 2f) - 6f)
            {
                EndGame();
                return;
            }
        }

        _timerLabel.Text = $"Survived: {_timeSurvived:F1}s";
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_gameOver) return;

        // Player
        DrawRect(new Rect2(_playerPos.X - PlayerW / 2f, _playerPos.Y - PlayerH / 2f, PlayerW, PlayerH),
                 new Color(0.2f, 0.6f, 1f));
        DrawString(ThemeDB.FallbackFont, _playerPos + new Vector2(-14f, 8f), "(^‿^)",
                   modulate: Colors.White, fontSize: 18);

        // Blocks — colour shifts redder as speed increases
        float danger = Mathf.Clamp(_timeSurvived / 30f, 0f, 1f);
        var blockColor = new Color(0.85f + danger * 0.15f, 0.25f - danger * 0.15f, 0.25f - danger * 0.1f);
        foreach (var (pos, _, w) in _blocks)
            DrawRect(new Rect2(pos.X - w / 2f, pos.Y - BlockH / 2f, w, BlockH), blockColor);
    }

    private void EndGame()
    {
        _gameOver = true;
        int score = Mathf.FloorToInt(_timeSurvived);
        _resultLabel.Text    = $"DEAD\n{_timeSurvived:F1} seconds survived\nScore: {score}";
        _resultLabel.Visible = true;
        _playAgainButton.Visible = true;
        _backButton.Visible  = true;
        QueueRedraw();
        SaveScore(score);
    }

    private void Restart()
    {
        GetTree().ReloadCurrentScene();
    }

    private void SaveScore(int score)
    {
        var data = new Godot.Collections.Dictionary
        {
            { "game",   "jonathan_dodge_robin" },
            { "player", "Jonathan" },
            { "score",  score }
        };

        DirAccess.MakeDirRecursiveAbsolute("user://scores");
        using var file = FileAccess.Open("user://scores/jonathan_dodge_robin.json", FileAccess.ModeFlags.Write);
        file.StoreString(Json.Stringify(data));
    }

    private void ReturnToLauncher()
    {
        GetTree().ChangeSceneToFile("res://launcher/Launcher.tscn");
    }
}
