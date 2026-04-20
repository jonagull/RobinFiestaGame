using Godot;

public partial class BikeEscape : Node2D
{
	[Export] public Vector2 GoalPosition  = new Vector2(-563f, 2175f);
	[Export] public Vector2 HomePosition  = new Vector2(-2143f, 3658f);

	private const float CatchDistance    = 35f;
	private const float GoalDistance     = 60f;
	private const float NicoUnlockDelay  = 5f;   // seconds before Nico button activates
	private const float NicoSlowDuration = 3f;  // seconds chasers are slowed

	private Robin       _robin;
	private Node2D      _chasers;
	private Label       _timerLabel;
	private Label       _distanceLabel;
	private Label       _statusLabel;
	private Control     _gameOverPanel;
	private Label       _gameOverLabel;
	private DialogueBox _dialogueBox;
	private Button      _bugslideButton;
	private Button      _throwButton;
	private Button      _nicoButton;
	private Button      _debugCarButton;
	private Chaser      _yuval;
	private Chaser      _are;
	private Chaser      _trygve;
	private Chaser      _viljar;

	private const float TrygveViljarUnlockDelay = 15f; // seconds before collision can trigger

	private float _elapsed         = 0f;
	private bool  _gameOver        = false;
	private bool  _debugImmune     = false;
	private float _nicoUnlockTimer = NicoUnlockDelay;
	private float _nicoSlowTimer   = 0f;
	private bool  _nicoUsed        = false;

	public override void _Ready()
	{
		_robin         = GetNode<Robin>("Robin");
		_chasers       = GetNode<Node2D>("Chasers");
		_timerLabel    = GetNode<Label>("HUD/TimerLabel");
		_distanceLabel = GetNode<Label>("HUD/DistanceLabel");
		_statusLabel   = GetNode<Label>("HUD/StatusLabel");
		_gameOverPanel = GetNode<Control>("HUD/GameOverPanel");
		_gameOverLabel = GetNode<Label>("HUD/GameOverPanel/GameOverLabel");
		_dialogueBox   = GetNode<DialogueBox>("HUD/DialogueBox");

		GetNode<Button>("HUD/GameOverPanel/RestartButton").Pressed += () => GetTree().ReloadCurrentScene();
		GetNode<Button>("HUD/GameOverPanel/BackButton").Pressed    += () => ReturnToLauncher();

		_yuval          = _chasers.GetNodeOrNull<Chaser>("Yuval");
		_are            = _chasers.GetNodeOrNull<Chaser>("Are");
		_trygve         = _chasers.GetNodeOrNull<Chaser>("Trygve");
		_viljar         = _chasers.GetNodeOrNull<Chaser>("Viljar");
		if (_trygve != null)
			_trygve.HomePath = GetNodeOrNull<Path2D>("HomePath");

		var bybane1 = _chasers.GetNodeOrNull<Chaser>("Bybanen");
		var bybane2 = _chasers.GetNodeOrNull<Chaser>("Bybanen2");
		if (bybane1 != null) bybane1.BybanePath = GetNodeOrNull<Path2D>("BybaneRoute1");
		if (bybane2 != null) bybane2.BybanePath = GetNodeOrNull<Path2D>("BybaneRoute2");
		_bugslideButton = GetNode<Button>("HUD/BugslideButton");
		_throwButton    = GetNode<Button>("HUD/ThrowButton");
		_nicoButton     = GetNode<Button>("HUD/NicoButton");

		_bugslideButton.Pressed += () => _yuval?.ActivateBugslide();
		_throwButton.Pressed    += () => _are?.ActivateThrow();
		_nicoButton.Pressed     += ActivateNico;

		_nicoButton.Disabled = true;

		_debugCarButton = new Button();
		_debugCarButton.Text = "[DBG] Car";
		_debugCarButton.AnchorLeft   = 0f;
		_debugCarButton.AnchorRight  = 0f;
		_debugCarButton.AnchorTop    = 0.5f;
		_debugCarButton.AnchorBottom = 0.5f;
		_debugCarButton.OffsetLeft   = 10f;
		_debugCarButton.OffsetRight  = 150f;
		_debugCarButton.OffsetTop    = 250f;
		_debugCarButton.OffsetBottom = 310f;
		_debugCarButton.GrowVertical = Control.GrowDirection.Both;
		_debugCarButton.Pressed += DebugTriggerCar;
		GetNode<CanvasLayer>("HUD").AddChild(_debugCarButton);

		var debugImmuneButton = new Button();
		debugImmuneButton.Text = "[DBG] Immune: OFF";
		debugImmuneButton.AnchorLeft   = 0f;
		debugImmuneButton.AnchorRight  = 0f;
		debugImmuneButton.AnchorTop    = 0.5f;
		debugImmuneButton.AnchorBottom = 0.5f;
		debugImmuneButton.OffsetLeft   = 10f;
		debugImmuneButton.OffsetRight  = 150f;
		debugImmuneButton.OffsetTop    = 320f;
		debugImmuneButton.OffsetBottom = 380f;
		debugImmuneButton.GrowVertical = Control.GrowDirection.Both;
		debugImmuneButton.Pressed += () => {
			_debugImmune = !_debugImmune;
			debugImmuneButton.Text = _debugImmune ? "[DBG] Immune: ON" : "[DBG] Immune: OFF";
		};
		GetNode<CanvasLayer>("HUD").AddChild(debugImmuneButton);

		_gameOverPanel.Visible = false;
		QueueRedraw();
	}

	public override void _Input(InputEvent ev)
	{
		if (_gameOver) return;
		if (ev is not InputEventKey key || !key.Pressed || key.Echo) return;

		switch (key.Keycode)
		{
			case Key.Key1: _are?.ActivateThrow();      break;
			case Key.Key2: _yuval?.ActivateBugslide(); break;
			case Key.Key3: ActivateNico();             break;
		}
	}

	private void ActivateNico()
	{
		if (_nicoUsed || _nicoUnlockTimer > 0f || _gameOver) return;
		_nicoUsed      = true;
		_nicoSlowTimer = NicoSlowDuration;
		_nicoButton.Disabled = true;

		foreach (Node child in _chasers.GetChildren())
			if (child is Chaser c) c.ApplySlow(NicoSlowDuration);

		ShowDialogue("Nico", null, "Vent litt, folkens!");
	}

	public override void _Process(double delta)
	{
		QueueRedraw();

		if (_gameOver) return;

		// Nico unlock countdown
		if (_nicoUnlockTimer > 0f)
		{
			_nicoUnlockTimer -= (float)delta;
			if (_nicoUnlockTimer <= 0f && !_nicoUsed)
				_nicoButton.Disabled = false;
		}

		if (_nicoSlowTimer > 0f)
			_nicoSlowTimer -= (float)delta;

		_elapsed += (float)delta;

		int m = (int)(_elapsed / 60f);
		int s = (int)(_elapsed % 60f);
		_timerLabel.Text = $"Time: {m:D2}:{s:D2}";

		float dist = _robin.GlobalPosition.DistanceTo(GoalPosition);
		_distanceLabel.Text = $"Dist: {(int)dist}px";

		if (_robin.IsHit)
			_statusLabel.Text = "HIT!";
		else
			_statusLabel.Text = _robin.IsOnWater ? "CANOE" : "BIKE";

		if (_yuval != null)
			_bugslideButton.Text = _yuval.BugslideActive
				? "[2] BUGSLIDE!\n(active)"
				: "[2] BUGSLIDE\n(Yuval)";

		if (_are != null)
			_throwButton.Text = _are.ThrowReady
				? "[1] THROW\n(Are)"
				: $"[1] THROW\n({_are.ThrowCooldownRemaining:F1}s)";

		if (!_nicoUsed)
			_nicoButton.Text = _nicoUnlockTimer > 0f
				? $"[3] NICO\n({_nicoUnlockTimer:F1}s...)"
				: "[3] NICO\n(slow them!)";
		else
			_nicoButton.Text = _nicoSlowTimer > 0f
				? $"[3] NICO\n🐌 {_nicoSlowTimer:F1}s"
				: "[3] NICO\n(used)";

		if (dist < GoalDistance)
		{
			EndGame(true);
			return;
		}

		// Trygve picks up Viljar (not in first 15 seconds)
		if (_elapsed >= TrygveViljarUnlockDelay
			&& _trygve != null && _viljar != null
			&& _trygve.State == Chaser.ChaseState.Normal
			&& _viljar.State == Chaser.ChaseState.Normal
			&& _trygve.GlobalPosition.DistanceTo(_viljar.GlobalPosition) < 40f)
		{
			_trygve.PickUpPassenger(_viljar);
			ShowDialogue("Viljar", null, "Kjør meg hjem, Trygve!");
		}

		if (!_debugImmune)
			foreach (Node child in _chasers.GetChildren())
			{
				if (child is Chaser chaser && chaser.GlobalPosition.DistanceTo(_robin.GlobalPosition) < CatchDistance)
				{
					EndGame(false);
					return;
				}
			}
	}

	public override void _Draw()
	{
		// Goal
		DrawCircle(GoalPosition, GoalDistance, new Color(0f, 1f, 0f, 0.25f));
		DrawArc(GoalPosition, GoalDistance, 0f, Mathf.Tau, 48, new Color(0f, 1f, 0f, 1f), 3f);

		// Robin's home
		DrawCircle(HomePosition, 40f, new Color(1f, 0.9f, 0f, 0.2f));
		DrawArc(HomePosition, 40f, 0f, Mathf.Tau, 48, new Color(1f, 0.9f, 0f, 1f), 3f);
	}

	private void EndGame(bool won)
	{
		_gameOver = true;
		_robin.SetPhysicsProcess(false);
		foreach (Node child in _chasers.GetChildren())
			child.SetPhysicsProcess(false);

		float dist = _robin.GlobalPosition.DistanceTo(GoalPosition);
		_gameOverLabel.Text = won
			? $"Made it!\n{(int)_elapsed}s"
			: $"Caught!\n{(int)dist}px from office";

		_gameOverPanel.Visible = true;
		SaveScore(won ? (int)_elapsed : 99999);
	}

	private void SaveScore(int score)
	{
		var data = new Godot.Collections.Dictionary
		{
			{ "game",   "viljar_bike_escape" },
			{ "player", "Viljar" },
			{ "score",  score }
		};
		DirAccess.MakeDirRecursiveAbsolute("user://scores");
		using var file = FileAccess.Open("user://scores/viljar_bike_escape.json", FileAccess.ModeFlags.Write);
		file.StoreString(Json.Stringify(data));
	}

	private void DebugTriggerCar()
	{
		if (_trygve == null || _viljar == null) return;
		if (_trygve.State != Chaser.ChaseState.Normal || _viljar.State != Chaser.ChaseState.Normal) return;
		_viljar.GlobalPosition = _trygve.GlobalPosition;
		_trygve.PickUpPassenger(_viljar);
		ShowDialogue("DEBUG", null, "Viljar in car — Trygve driving home");
	}

	public void ShowDialogue(string characterName, Texture2D portrait, string text)
	{
		_dialogueBox?.Show(characterName, portrait, text);
	}

	protected void ReturnToLauncher()
	{
		GetTree().ChangeSceneToFile("res://launcher/Launcher.tscn");
	}
}
