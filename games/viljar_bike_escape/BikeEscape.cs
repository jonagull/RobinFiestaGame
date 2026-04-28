using Godot;

public partial class BikeEscape : Node2D
{
	[Export] public Vector2 GoalPosition = new Vector2(-563f, 2175f);  // Rubus office
	[Export] public Vector2 HomePosition = new Vector2(-2143f, 3658f);

	private Marker2D _rubusMarker;
	private Marker2D _tidsbankenMarker;
	private Marker2D _golfCourseMarker;
	private Marker2D _tirilMarker;

	private const float CatchDistance    = 35f;
	private const float GoalDistance     = 60f;
	private const float NicoUnlockDelay  = 5f;   // seconds before Nico button activates
	private const float NicoSlowDuration = 3f;  // seconds chasers are slowed
	private const float GolfBallCooldown = 8f;

	private Robin       _robin;
	private Node2D      _chasers;
	private Label       _timerLabel;
	private Label       _distanceLabel;
	private Label       _statusLabel;
	private Control     _gameOverPanel;
	private Label       _gameOverLabel;
	private DialogueBox   _dialogueBox;
	private StoryDialogue _storyDialogue;
	private Button      _bugslideButton;
	private Button      _throwButton;
	private Button      _nicoButton;
	private Button      _riceButton;
	private Button      _wineButton;
	private Button      _golfButton;
	private Button             _debugCarButton;
	private ObjectiveArrow     _objectiveArrow;
	private VideoStreamPlayer  _videoPlayer;
	private float              _videoCooldown = 0f;
	private Chaser      _yuval;
	private Chaser      _are;
	private Chaser      _trygve;
	private Chaser      _viljar;
	private Chaser      _vegard;
	private Chaser      _guy;

	private const float TrygveViljarUnlockDelay = 15f; // seconds before collision can trigger

	private float _elapsed           = 0f;
	private float _golfCooldownTimer = 0f;
	private bool  _gameOver        = false;
	private int   _storyStage      = 0;   // 0=intro, 1=heading home, 2=heading to golf, 3=heading to Tiril
	private bool  _transitioning   = false;
	private bool  _debugImmune     = false;
	private float _nicoUnlockTimer = NicoUnlockDelay;
	private float _nicoSlowTimer   = 0f;
	private bool  _nicoUsed        = false;

	public override void _Ready()
	{
		_rubusMarker      = GetNodeOrNull<Marker2D>("RubusMarker");
		_tidsbankenMarker = GetNodeOrNull<Marker2D>("TidsbankenMarker");
		_golfCourseMarker = GetNodeOrNull<Marker2D>("GolfCourseMarker");
		_tirilMarker      = GetNodeOrNull<Marker2D>("TirilMarker");

		if (_rubusMarker != null) GoalPosition = _rubusMarker.GlobalPosition;

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
		_vegard         = _chasers.GetNodeOrNull<Chaser>("Vegard");
		_guy            = _chasers.GetNodeOrNull<Chaser>("Guy");
		if (_trygve != null)
			_trygve.HomePath = GetNodeOrNull<Path2D>("HomePath");

		var bybane1 = _chasers.GetNodeOrNull<Chaser>("Bybanen");
		var bybane2 = _chasers.GetNodeOrNull<Chaser>("Bybanen2");
		if (bybane1 != null) bybane1.BybanePath = GetNodeOrNull<Path2D>("BybaneRoute1");
		if (bybane2 != null) bybane2.BybanePath = GetNodeOrNull<Path2D>("BybaneRoute2");
		_bugslideButton = GetNode<Button>("HUD/BugslideButton");
		_throwButton    = GetNode<Button>("HUD/ThrowButton");
		_nicoButton     = GetNode<Button>("HUD/NicoButton");
		_riceButton     = GetNode<Button>("HUD/RiceButton");
		_wineButton     = GetNode<Button>("HUD/WineButton");
		_golfButton     = GetNode<Button>("HUD/GolfButton");

		_bugslideButton.Pressed += () => _yuval?.ActivateBugslide();
		_throwButton.Pressed    += () => _are?.ActivateThrow();
		_nicoButton.Pressed     += ActivateNico;
		_riceButton.Pressed     += () => _vegard?.ActivateRiceThrow();
		_wineButton.Pressed     += () => _guy?.ActivateWine();
		_golfButton.Pressed     += DropGolfBalls;

		_nicoButton.Disabled = true;

		_debugCarButton = new Button();
		_debugCarButton.Text = "[DBG] Car";
		_debugCarButton.AnchorLeft   = 1f;
		_debugCarButton.AnchorRight  = 1f;
		_debugCarButton.AnchorTop    = 0.5f;
		_debugCarButton.AnchorBottom = 0.5f;
		_debugCarButton.OffsetLeft   = -150f;
		_debugCarButton.OffsetRight  = -10f;
		_debugCarButton.OffsetTop    = -80f;
		_debugCarButton.OffsetBottom = -20f;
		_debugCarButton.GrowVertical = Control.GrowDirection.Both;
		_debugCarButton.Pressed += DebugTriggerCar;
		GetNode<CanvasLayer>("HUD").AddChild(_debugCarButton);

		var debugVideoButton = new Button();
		debugVideoButton.Text = "[DBG] Video";
		debugVideoButton.AnchorLeft   = 1f;
		debugVideoButton.AnchorRight  = 1f;
		debugVideoButton.AnchorTop    = 0.5f;
		debugVideoButton.AnchorBottom = 0.5f;
		debugVideoButton.OffsetLeft   = -150f;
		debugVideoButton.OffsetRight  = -10f;
		debugVideoButton.OffsetTop    = -150f;
		debugVideoButton.OffsetBottom = -90f;
		debugVideoButton.GrowVertical = Control.GrowDirection.Both;
		debugVideoButton.Pressed += () => PlayVideo("res://games/viljar_bike_escape/assets/video/swimming_time.ogv");
		GetNode<CanvasLayer>("HUD").AddChild(debugVideoButton);

		var debugImmuneButton = new Button();
		debugImmuneButton.Text = "[DBG] Immune: OFF";
		debugImmuneButton.AnchorLeft   = 1f;
		debugImmuneButton.AnchorRight  = 1f;
		debugImmuneButton.AnchorTop    = 0.5f;
		debugImmuneButton.AnchorBottom = 0.5f;
		debugImmuneButton.OffsetLeft   = -150f;
		debugImmuneButton.OffsetRight  = -10f;
		debugImmuneButton.OffsetTop    = -10f;
		debugImmuneButton.OffsetBottom = 50f;
		debugImmuneButton.GrowVertical = Control.GrowDirection.Both;
		debugImmuneButton.Pressed += () => {
			_debugImmune = !_debugImmune;
			debugImmuneButton.Text = _debugImmune ? "[DBG] Immune: ON" : "[DBG] Immune: OFF";
		};
		GetNode<CanvasLayer>("HUD").AddChild(debugImmuneButton);

		var vpSize = GetViewport().GetVisibleRect().Size;
		var videoContainer          = new Control();
		videoContainer.ClipContents = true;
		videoContainer.Size         = new Vector2(320f, 180f);
		videoContainer.Position     = new Vector2(vpSize.X - 320f, 0f);
		videoContainer.Visible      = false;
		videoContainer.ProcessMode  = ProcessModeEnum.Always;
		GetNode<CanvasLayer>("HUD").AddChild(videoContainer);

		_videoPlayer                 = new VideoStreamPlayer();
		_videoPlayer.AnchorRight     = 1f;
		_videoPlayer.AnchorBottom    = 1f;
		_videoPlayer.OffsetLeft      = 0f;
		_videoPlayer.OffsetTop       = 0f;
		_videoPlayer.OffsetRight     = 0f;
		_videoPlayer.OffsetBottom    = 0f;
		_videoPlayer.Expand          = true;
		_videoPlayer.ProcessMode     = ProcessModeEnum.Always;
		_videoPlayer.Finished       += () => { videoContainer.Visible = false; _videoCooldown = 10f; };
		videoContainer.AddChild(_videoPlayer);

		_objectiveArrow              = new ObjectiveArrow();
		_objectiveArrow.GoalWorldPos = GoalPosition;
		_objectiveArrow.Robin        = _robin;
		_objectiveArrow.Camera       = _robin.GetNodeOrNull<Camera2D>("Camera2D");
		GetNode<CanvasLayer>("HUD").AddChild(_objectiveArrow);

		var music = new AudioStreamPlayer();
		music.Stream    = ResourceLoader.Load<AudioStream>("res://games/viljar_bike_escape/assets/songs/VegardSong.ogg");
		music.Autoplay  = true;
		music.VolumeDb  = -6f;
		music.ProcessMode = ProcessModeEnum.Always;
		if (music.Stream is AudioStreamOggVorbis ogg) ogg.Loop = true;
		AddChild(music);

		_gameOverPanel.Visible = false;
		QueueRedraw();

		// Chasers stay frozen until Robin leaves the office
		foreach (Node child in _chasers.GetChildren())
			child.SetPhysicsProcess(false);

		var storyScene = GD.Load<PackedScene>("res://games/viljar_bike_escape/StoryDialogue.tscn");
		_storyDialogue = storyScene.Instantiate<StoryDialogue>();
		GetNode<CanvasLayer>("HUD").AddChild(_storyDialogue);
		_transitioning = true;
		_storyDialogue.ScriptFinished += OnDialogueFinished;
		GetTree().Paused = true;
		_storyDialogue.PlayScript("res://games/viljar_bike_escape/story/intro.json");
	}

	private void TriggerArrival()
	{
		_transitioning = true;
		_storyStage++;

		var (path, nextGoal) = _storyStage switch
		{
			1 => ("res://games/viljar_bike_escape/story/rubus_arrival.json", HomePosition),
			2 => ("res://games/viljar_bike_escape/story/home_arrival.json",  _golfCourseMarker?.GlobalPosition ?? Vector2.Zero),
			3 => ("res://games/viljar_bike_escape/story/golf_arrival.json",  _tirilMarker?.GlobalPosition      ?? Vector2.Zero),
			4 => ("res://games/viljar_bike_escape/story/tiril_arrival.json", Vector2.Zero),
			_ => (null, Vector2.Zero)
		};

		if (path != null && FileAccess.FileExists(path))
		{
			GetTree().Paused = true;
			_storyDialogue.PlayScript(path);
			if (nextGoal != Vector2.Zero)
				GoalPosition = nextGoal;
		}
		else
		{
			EndGame(true);
		}
	}

	private void OnDialogueFinished()
	{
		_transitioning = false;
		// Release the chasers once Robin leaves the office
		if (_storyStage == 1)
			foreach (Node child in _chasers.GetChildren())
				child.SetPhysicsProcess(true);
		GetTree().Paused = false;
	}

	public override void _Input(InputEvent ev)
	{
		if (_gameOver) return;
		if (ev is not InputEventKey key || !key.Pressed || key.Echo) return;

		switch (key.Keycode)
		{
			case Key.Key1: _are?.ActivateThrow();        break;
			case Key.Key2: _yuval?.ActivateBugslide();   break;
			case Key.Key3: ActivateNico();               break;
			case Key.Key4: _vegard?.ActivateRiceThrow(); break;
			case Key.Key5: _guy?.ActivateWine();         break;
			case Key.Key6: DropGolfBalls();              break;
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

		if (_golfCooldownTimer > 0f) _golfCooldownTimer -= (float)delta;
		if (_videoCooldown     > 0f) _videoCooldown     -= (float)delta;
		if (_objectiveArrow != null) _objectiveArrow.GoalWorldPos = GoalPosition;
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

		if (_vegard != null)
		{
			if (!_vegard.HasStarted)
				_riceButton.Text = "[4] RICE\n(joining...)";
			else if (_vegard.RiceReady)
				_riceButton.Text = "[4] RICE\n(Vegard)";
			else
				_riceButton.Text = $"[4] RICE\n({_vegard.RiceCooldownRemaining:F1}s)";
		}

		if (_guy != null)
		{
			if (_guy.IsDrunk)
				_wineButton.Text = $"[5] WINE\n*hic* {_guy.WineDrunkRemaining:F1}s";
			else if (_guy.WineReady)
				_wineButton.Text = "[5] WINE\n(Guy)";
			else
				_wineButton.Text = $"[5] WINE\n({_guy.WineCooldownRemaining:F1}s)";
		}

		_golfButton.Text = _golfCooldownTimer > 0f
			? $"[6] GOLF\n({_golfCooldownTimer:F1}s)"
			: "[6] GOLF\n⛳ drop!";

		if (dist < GoalDistance && !_transitioning)
		{
			TriggerArrival();
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

		// Story stage markers
		DrawStageMarker(_tidsbankenMarker?.GlobalPosition, "Tidsbanken", new Color(0.4f, 0.8f, 1f));
		DrawStageMarker(_golfCourseMarker?.GlobalPosition, "Golf",       new Color(0.3f, 0.9f, 0.3f));
		DrawStageMarker(_tirilMarker?.GlobalPosition,      "Tiril",      new Color(1f,   0.5f, 0.8f));
	}

	private void DrawStageMarker(Vector2? pos, string label, Color color)
	{
		if (pos == null || pos == Vector2.Zero) return;
		var p = pos.Value;
		DrawCircle(p, GoalDistance, color with { A = 0.2f });
		DrawArc(p, GoalDistance, 0f, Mathf.Tau, 48, color, 3f);
		DrawString(ThemeDB.FallbackFont, p + new Vector2(-20f, -GoalDistance - 6f), label,
			HorizontalAlignment.Left, -1, 13, color);
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

	private void DropGolfBalls()
	{
		if (_golfCooldownTimer > 0f || _gameOver) return;
		_golfCooldownTimer = GolfBallCooldown;

		var offsets = new Vector2[]
		{
			new Vector2(  0f,   0f),
			new Vector2(-20f,  14f),
			new Vector2( 20f,  14f),
			new Vector2(-10f, -14f),
			new Vector2( 10f, -14f),
		};

		var backDir = _robin.Velocity.LengthSquared() > 1f
			? -_robin.Velocity.Normalized()
			: Vector2.Down;

		foreach (var off in offsets)
		{
			var ball = new GolfBall();
			AddChild(ball);
			ball.GlobalPosition = _robin.GlobalPosition + off;
			float spread = (float)GD.RandRange(-0.45f, 0.45f);
			ball.Init(backDir.Rotated(spread) * (float)GD.RandRange(70f, 140f));
		}
	}

	public void PlayVideo(string path)
	{
		if (_videoPlayer.IsPlaying() || _videoCooldown > 0f) return;
		var stream = ResourceLoader.Load<VideoStream>(path);
		if (stream == null) { GD.PrintErr($"[Video] Failed to load: {path}"); return; }
		_videoPlayer.Stream = stream;
		_videoPlayer.GetParent<Control>().Visible = true;
		_videoPlayer.Play();
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
