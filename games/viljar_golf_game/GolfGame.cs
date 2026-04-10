using Godot;

public partial class GolfGame : Node
{
	enum State { Aiming, Power, Swinging, BallFlying, Reacting }

	const int TotalShots = 5;
	const float AimMinX = 200f;
	const float AimMaxX = 900f;
	const float AimSpeed = 220f;

	State _state = State.Aiming;
	float _aimX = 550f;
	float _aimDir = 1f;
	float _lockedAimX = 0f;
	float _lockedPower = 0f;
	float _powerValue = 0f;
	int _shotsLeft = TotalShots;
	int _score = 0;

	bool _robinMoving = false;
	float _robinMoveSpeed = 0f;

	Sprite2D _golfer;
	Sprite2D _ball;
	Sprite2D _robinTarget;
	Line2D _aimLine;
	ProgressBar _powerBar;
	Label _powerBarLabel;
	Label _instructionLabel;
	Label _shotsLabel;
	Label _scoreLabel;
	Label _resultLabel;

	Texture2D[] _swingFrames = new Texture2D[5];
	int _swingFrame = 0;
	float _swingTimer = 0f;
	float _pauseTimer = 0f;
	Sprite2D _hitMarker;
	Image _robinPixels; // cached for pixel-perfect hit detection

	Vector2 _ballStart;
	Vector2 _flightStart;
	Vector2 _flightEnd;
	float _flightProgress;
	float _flightDuration;
	float _flightMaxHeight;

	// Each entry: (photo path, should move)
	readonly (string photo, bool moves)[] _rounds = new[]
	{
		("res://games/viljar_golf_game/assets/robin/DSC09016-removebg-preview.png",  false),
		("res://games/viljar_golf_game/assets/robin/DSC00920-removebg-preview.png",  false),
		("res://games/viljar_golf_game/assets/robin/DSC04104-2-removebg-preview.png", true),
		("res://games/viljar_golf_game/assets/robin/DSC00958-2-removebg-preview.png", true),
		("res://games/viljar_golf_game/assets/robin/DSC09016-removebg-preview.png",  true),
	};

	public override void _Ready()
	{
		_golfer           = GetNode<Sprite2D>("World/Golfer");
		_ball             = GetNode<Sprite2D>("World/Ball");
		_robinTarget      = GetNode<Sprite2D>("World/RobinTarget");
		_aimLine          = GetNode<Line2D>("World/AimLine");
		_powerBar         = GetNode<ProgressBar>("UI/PowerBar");
		_powerBarLabel    = GetNode<Label>("UI/PowerBarLabel");
		_instructionLabel = GetNode<Label>("UI/InstructionLabel");
		_shotsLabel       = GetNode<Label>("UI/ShotsLabel");
		_scoreLabel       = GetNode<Label>("UI/ScoreLabel");
		_resultLabel      = GetNode<Label>("UI/ResultLabel");

		GetNode<Button>("UI/BackButton").Pressed += ReturnToLauncher;

		for (int i = 1; i <= 5; i++)
			_swingFrames[i - 1] = GD.Load<Texture2D>(
				$"res://games/viljar_golf_game/assets/Dinky_Tiny_Golf_Free/Singles/Player/Swing0{i}.png");

		_ballStart = _ball.Position;
		SetupRound();
	}

	void SetupRound()
	{
		int roundIndex = TotalShots - _shotsLeft;
		var (photo, moves) = _rounds[roundIndex];

		_robinTarget.Texture = GD.Load<Texture2D>(photo);
		_robinPixels = _robinTarget.Texture.GetImage();
		_robinTarget.Visible = true;
		_robinTarget.Modulate = Colors.White;
		_robinTarget.Rotation = -0.08f; // slight lean toward camera

		// Distance tier: 0=close, 1=medium, 2=far — affects Y and scale
		int tier = roundIndex < 2 ? 0 : (roundIndex < 4 ? 1 : 2);
		float robinY = tier == 0 ? 380f : (tier == 1 ? 300f : 230f);
		float robinScale = tier == 0 ? 0.55f : (tier == 1 ? 0.42f : 0.32f);
		_robinTarget.Scale = new Vector2(robinScale, robinScale);
		_robinTarget.Position = new Vector2((float)GD.RandRange(250, 850), robinY);

		_robinMoving = moves;
		_robinMoveSpeed = moves ? (float)GD.RandRange(80, 170) : 0f;

		_aimX = 550f;
		_aimDir = 1f;
		_aimLine.Visible = true;
		_ball.Position = _ballStart;
		_ball.Scale = new Vector2(3.5f, 3.5f);
		_ball.ZIndex = 1;
		_ball.Visible = true;

		_hitMarker?.QueueFree();
		_hitMarker = null;

		_golfer.Texture = _swingFrames[0];
		_powerBar.Value = 0;
		_resultLabel.Text = "";
		_state = State.Aiming;

		_shotsLabel.Text = $"Shots left: {_shotsLeft}";
		_scoreLabel.Text = $"Hits: {_score}";
		_instructionLabel.Text = "SPACE — aim!";
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;

		switch (_state)
		{
			case State.Aiming:
				_aimX += _aimDir * AimSpeed * dt;
				if (_aimX >= AimMaxX) { _aimX = AimMaxX; _aimDir = -1f; }
				if (_aimX <= AimMinX) { _aimX = AimMinX; _aimDir =  1f; }
				_aimLine.SetPointPosition(0, new Vector2(_aimX, 180f));
				_aimLine.SetPointPosition(1, new Vector2(_aimX, 500f));

				if (_robinMoving)
				{
					var pos = _robinTarget.Position;
					pos.X += _robinMoveSpeed * dt;
					if (pos.X > 860f || pos.X < 220f) _robinMoveSpeed = -_robinMoveSpeed;
					_robinTarget.Position = pos;
				}
				break;

			case State.Power:
				_powerValue += 80f * dt;
				if (_powerValue >= 100f) _powerValue = 100f;
				_powerBar.Value = _powerValue;
				break;

			case State.Swinging:
				_swingTimer += dt;
				if (_swingTimer >= 0.1f)
				{
					_swingTimer = 0f;
					_swingFrame++;
					if (_swingFrame < _swingFrames.Length)
						_golfer.Texture = _swingFrames[_swingFrame];
					else
						LaunchBall();
				}
				break;

			case State.BallFlying:
				_flightProgress = Mathf.Min(_flightProgress + dt / _flightDuration, 1f);
				float t = _flightProgress;

				// Ground track: straight line from tee to landing spot
				Vector2 groundPos = _flightStart.Lerp(_flightEnd, t);

				// Height arc: rises then falls back to ground
				float heightOffset = _flightMaxHeight * Mathf.Sin(Mathf.Pi * t);

				// Perspective scale: big near camera, small in distance
				float ballScale = Mathf.Lerp(3.5f, 0.8f, t);

				_ball.Position = new Vector2(groundPos.X, groundPos.Y - heightOffset);
				_ball.Scale = new Vector2(ballScale, ballScale);
				// Draw ball in front when closer to camera than Robin, behind when past Robin
				_ball.ZIndex = _ball.Position.Y > _robinTarget.Position.Y ? 1 : -1;

				if (t >= 1f)
					CheckHit();
				break;

			case State.Reacting:
				_pauseTimer -= dt;
				if (_pauseTimer <= 0f)
					NextRound();
				break;
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventKey key || !key.Pressed || key.Echo) return;
		if (key.Keycode != Key.Space) return;

		switch (_state)
		{
			case State.Aiming:
				_lockedAimX = _aimX;
				_aimLine.Visible = false;
				_state = State.Power;
				_powerValue = 0f;
				_instructionLabel.Text = "SPACE — lock power!";
				break;

			case State.Power:
				_lockedPower = _powerValue;
				_state = State.Swinging;
				_swingFrame = 0;
				_swingTimer = 0f;
				_instructionLabel.Text = "";
				break;
		}
	}

	void LaunchBall()
	{
		_state = State.BallFlying;

		float power = _lockedPower / 100f;

		_flightProgress = 0f;
		_flightDuration = 0.7f + power * 0.8f;          // weak = short flight, strong = longer
		_flightStart = _ballStart;
		_flightEnd = new Vector2(
			_lockedAimX,
			Mathf.Lerp(440f, 210f, power)                // more power = further into screen
		);
		_flightMaxHeight = 60f + power * 60f;            // bigger arc for harder hits

		_ball.Position = _ballStart;
		_ball.Visible = true;
	}

	bool IsPixelHit(Vector2 worldPos)
	{
		if (_robinPixels == null) return true;

		// Convert world position to Robin's local texture space
		Vector2 local = worldPos - _robinTarget.Position;
		local = local.Rotated(-_robinTarget.Rotation);
		local /= _robinTarget.Scale;

		int w = _robinPixels.GetWidth();
		int h = _robinPixels.GetHeight();
		int px = Mathf.RoundToInt(local.X + w / 2f);
		int py = Mathf.RoundToInt(local.Y + h / 2f);

		if (px < 0 || py < 0 || px >= w || py >= h) return false;
		return _robinPixels.GetPixel(px, py).A > 0.1f;
	}

	void CheckHit()
	{
		_ball.Visible = false;

		Vector2 ballLanding = _flightEnd;
		float dist = ballLanding.DistanceTo(_robinTarget.Position);
		float hitRadius = 90f;

		if (dist < hitRadius && IsPixelHit(ballLanding))
		{
			_score++;
			_scoreLabel.Text = $"Hits: {_score}";
			_resultLabel.Text = "HIT!";
			_resultLabel.Modulate = new Color(1f, 0.2f, 0.2f);
			SpawnHitmarker(_robinTarget.Position);
			PlayHitAnimation();
		}
		else
		{
			_resultLabel.Text = "miss...";
			_resultLabel.Modulate = Colors.White;
			_state = State.Reacting;
			_pauseTimer = 1.2f;
		}
	}

	void SpawnHitmarker(Vector2 screenPos)
	{
		_hitMarker?.QueueFree();

		_hitMarker = new Sprite2D();
		_hitMarker.Texture = GD.Load<Texture2D>("res://games/viljar_golf_game/assets/hit_marker.png");
		_hitMarker.Scale = new Vector2(0.04f, 0.04f);
		_hitMarker.Position = screenPos;
		GetNode<CanvasLayer>("World").AddChild(_hitMarker);
	}

	void PlayHitAnimation()
	{
		// Wall tipping backward — base stays fixed, top falls away
		// Keep base grounded: as Y scale collapses, center drifts down by half the sprite height
		float halfHeight = _robinTarget.Scale.Y * 250f; // ~500px tall source image
		Vector2 groundedPos = new Vector2(_robinTarget.Position.X, _robinTarget.Position.Y + halfHeight);

		var tween = CreateTween();
		tween.TweenProperty(_robinTarget, "scale:y", 0f, 0.45f)
			.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		tween.Parallel().TweenProperty(_robinTarget, "position", groundedPos, 0.45f)
			.SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(OnHitAnimDone));
	}

	void OnHitAnimDone()
	{
		_state = State.Reacting;
		_pauseTimer = 0.5f;
	}

	void NextRound()
	{
		_shotsLeft--;

		if (_shotsLeft <= 0)
		{
			ShowFinalResult();
			return;
		}

		SetupRound();
	}

	void ShowFinalResult()
	{
		_robinTarget.Visible = false;
		_aimLine.Visible = false;
		_resultLabel.Text = "";

		string summary;
		if (_score == TotalShots)
			summary = $"{_score}/{TotalShots} — Perfect round!";
		else if (_score >= 3)
			summary = $"{_score}/{TotalShots} — Not bad!";
		else if (_score >= 1)
			summary = $"{_score}/{TotalShots} — Robin is safe... for now.";
		else
			summary = $"{_score}/{TotalShots} — Did you even try?";

		_instructionLabel.Text = summary;
		_state = State.Reacting;
		_pauseTimer = float.MaxValue; // wait for back button
	}

	private void ReturnToLauncher()
	{
		GetTree().ChangeSceneToFile("res://launcher/Launcher.tscn");
	}
}
