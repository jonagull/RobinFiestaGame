using Godot;

public partial class Chaser : CharacterBody2D
{
	[Export] public float     Speed         = 195f;
	[Export] public float     TurnSpeed     = 3.5f; // radians per second
	[Export] public float     StartDelay    = 3f;
	[Export] public string    CharacterName = "Chaser";
	[Export] public Texture2D Portrait;
	[Export] public bool      IsVehicle     = false;
	[Export] public bool      IsTent        = false;
	[Export] public bool      HasBugslide      = false;
	[Export] public bool      HasThrow         = false;
	[Export] public bool      HasEnergyDrink   = false;
	[Export] public bool      HasRice          = false;
	[Export] public float     WaterSpeedMult   = 0.45f; // override per chaser
	[Export] public string    WaterEntryLine   = "";    // said once on entering water
	[Export] public string    WaterEntryVideo  = "";    // video played once on entering water
	[Export] public bool      IsBybane          = false;
	[Export] public int       BybaneFrameOffset = 0;   // tune if frames point the wrong way
	[Export] public Path2D    BybanePath        = null; // rail route — leave null to fall back to chasing Robin
	[Export] public Texture2D AltFrame         = null; // second frame for running animation

	[Export] public string[] Lines =
	{
		"Robin, come back!",
		"We need to talk!!",
		"It's not too late to stay!",
		"FREE COFFEE IF YOU RETURN",
		"Robin!!!",
		"You forgot your charger!",
	};

	private const float CooldownMin = 6f;
	private const float CooldownMax = 14f;

	// Bugslide
	private const float JumpDuration      = 0.5f;
	private const float SlideDuration     = 4.0f;
	private const float BugslideSpeedMult = 3.2f;
	private const float BugslideTurnRate  = 0.6f;   // rad/s — hard to steer during slide
	private const float BugslideScale     = 1.9f;   // airborne scale multiplier
	private const float SlideDecayNormal  = 1.0f;   // timer rate when going straight
	private const float SlideDecayTurning = 0.25f;  // timer rate when arcing (sustains slide)

	private enum BugslideState { None, Jump, Slide }
	private BugslideState _bsState        = BugslideState.None;

	public enum ChaseState { Normal, WaitingAtHome, InCar, DrivingHome }
	public ChaseState State               = ChaseState.Normal;
	private float         _bsTimer        = 0f;
	private float         _jumpRotStart;
	private float         _jumpRotTarget;
	private Vector2       _jumpTravelDir; // direction of travel during the air phase
	private float         _slideAngle;    // actual movement angle during slide — starts at original, lerps to 90°

	public bool BugslideActive => _bsState != BugslideState.None;

	private const float ThrowCooldown = 3.0f;
	private float       _throwTimer   = 0f;
	public  bool        ThrowReady            => HasThrow && _throwTimer <= 0f && _delay <= 0f;
	public  float       ThrowCooldownRemaining => _throwTimer;

	// Nico slow
	private const float SlowMult  = 0.18f;
	private float       _slowTimer = 0f;
	public  bool        IsSlowed   => _slowTimer > 0f;
	public  void        ApplySlow(float duration) => _slowTimer = duration;

	// Energy drink boost
	private const float EnergyDrinkDuration  = 5f;
	private const float EnergyDrinkSpeedMult = 1.35f;
	private float       _energyTimer          = 0f;
	public  bool        IsEnergized           => _energyTimer > 0f;
	public void ActivateEnergyDrink()
	{
		_energyTimer = EnergyDrinkDuration;
		_game?.ShowDialogue(CharacterName, Portrait, "DATABRUS!!!");
		ShowSpeechBubble("DATABRUS!!!");
	}

	// Golf ball spin
	private const float SpinDuration    = 1.6f;          // seconds for 2 full rotations
	private const float SpinRate        = Mathf.Tau * 2f / SpinDuration; // rad/s
	private float       _spinTimer      = 0f;
	public  bool        IsSpinning      => _spinTimer > 0f;
	public void ApplyGolfballSpin()
	{
		_spinTimer = SpinDuration;
		ShowSpeechBubble("⛳ !!");
	}

	// GI spike (from Vegard's rice)
	private const float GiSpikeMult = 1.2f;
	private float       _giSpikeTimer      = 0f;
	private float       _giSpikeTotalDuration = 0f;
	public  bool        IsGiSpiked         => _giSpikeTimer > 0f;
	public void ApplyGiSpike(float duration)
	{
		_giSpikeTimer         = duration;
		_giSpikeTotalDuration = duration;
	}

	// Wine — Guy shares wine with everyone nearby (AoE buff: faster but wobbly)
	private const float WineDuration    = 9f;
	private const float WineCooldown    = 15f;
	private const float WineAoeRadius   = 350f;
	private const float WineAoeDuration = 0.7f;   // expanding circle animation
	private const float DrunkSpeedMult     = 1.3f;
	private const float DrunkPhaseDuration = 3f;     // seconds before picking new cone angle
	private const float DrunkConeHalf      = Mathf.Pi / 18f; // ±10° cone toward Robin
	private float       _drunkPhaseTimer   = 0f;
	private float       _drunkFixedAngle   = 0f;     // committed angle for current phase

	// Per-chaser drunk state (set by ApplyDrunk, affects any chaser)
	private float _wineDrunkTimer = 0f;
	public  bool  IsDrunk         => _wineDrunkTimer > 0f;
	public void ApplyDrunk(float duration)
	{
		_wineDrunkTimer  = duration;
		_drunkPhaseTimer = 0f; // force a fresh pick immediately
	}

	// Guy's button cooldown (only relevant on HasWine chaser)
	private float _wineCooldownTimer = 0f;
	private float _wineAoeTimer      = 0f;
	public  bool  WineReady             => _wineCooldownTimer <= 0f;
	public  float WineCooldownRemaining => _wineCooldownTimer;
	public  float WineDrunkRemaining    => _wineDrunkTimer;

	public void ActivateWine()
	{
		if (!WineReady) return;
		_wineCooldownTimer = WineCooldown;
		_wineAoeTimer      = WineAoeDuration;
		ShowSpeechBubble("Skaal, alle saman!!");
		_game?.ShowDialogue(CharacterName, Portrait, "Skaal, alle saman!!");

		// Share wine with all chasers in radius
		foreach (Node sibling in GetParent().GetChildren())
		{
			if (sibling is not Chaser other) continue;
			if (GlobalPosition.DistanceTo(other.GlobalPosition) <= WineAoeRadius)
				other.ApplyDrunk(WineDuration);
		}
	}

	// Orbit (Stian)
	[Export] public bool  HasOrbit    = false;
	[Export] public float OrbitRadius = 220f;
	[Export] public float OrbitSpeed  = 1.3f;   // rad/s
	private float _orbitAngle = 0f;

	// Figma shot (Stian)
	[Export] public bool  HasFigmaShot     = false;
	private const  float FigmaShotCooldown = 2.5f;
	private float        _figmaShotTimer   = 0f;

	// Rice throw (Vegard)
	private const float RiceCooldown = 4f;
	private float       _riceTimer   = 0f;
	public  bool        HasStarted => _delay <= 0f;
	public  bool        RiceReady  => _riceTimer <= 0f;
	public  float       RiceCooldownRemaining => _riceTimer;

	// Water colour
	private static readonly Color WaterColor     = new Color(0.667f, 0.827f, 0.875f);
	private const           float ColorTolerance = 0.12f;

	private Robin               _robin;
	private BikeEscape          _game;
	private Sprite2D            _sprite;
	private CollisionShape2D    _collisionShape;
	private Vector2             _baseScale;
	private Texture2D           _baseFrame;
	private Texture2D[]         _bybaneFrames;
	private float      _delay;
	private float      _dialogueTimer;
	private Image      _mapImage;
	private int        _mapW;
	private int        _mapH;
	private float      _animTimer  = 0f;
	private bool       _animToggle = false;
	private bool       _wasOnWater  = false;
	private const float AnimFps     = 8f;

	// Trygve-carries-Viljar mechanic
	public  Chaser    CarryingPassenger   = null;
	public  Path2D    HomePath            = null;
	private Chaser    _driver             = null;
	private int       _waypointIndex      = 0;
	private float     _homeWaitTimer      = 0f;
	private const float WaypointReachDist  = 50f;
	private const float HomeArrivalDist    = 60f;
	private const float HomeWaitDuration   = 5f;

	// Bybane momentum
	private const float BybaneMaxSpeed        = 340f;
	private const float BybaneAccel           = 55f;   // px/s² when going straight
	private const float BybaneDecel           = 260f;  // px/s² when turning
	private const float BybaneMinSpeed        = 40f;   // crawl speed mid-turn
	private const float BybaneWpReachDist     = 28f;   // waypoint snap distance
	private float       _bybaneSpeed          = 0f;
	private int         _bybaneWpIndex        = 0;
	private int         _bybaneWpDir          = 1;     // +1 forward, -1 backward (ping-pong)

	private const float SpeechBubbleDuration = 3.5f;
	private const float SpeechBubbleFadeTime = 1.0f;
	private string _bubbleText  = null;
	private float  _bubbleTimer = 0f;

	public override void _Ready()
	{
		_delay         = StartDelay;
		_robin         = GetParent().GetParent().GetNodeOrNull<Robin>("Robin");
		_game          = GetParent().GetParent() as BikeEscape;
		_dialogueTimer = (float)GD.RandRange(CooldownMin, CooldownMax);
		_sprite         = GetNodeOrNull<Sprite2D>("Sprite2D");
		_collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		_baseScale      = _sprite?.Scale ?? Vector2.One;
		_baseFrame      = _sprite?.Texture;

		if (IsBybane)
		{
			_bybaneSpeed  = Speed;
			_bybaneFrames = new Texture2D[20];
			for (int i = 0; i < 20; i++)
			{
				int n = 14 + i;
				_bybaneFrames[i] = ResourceLoader.Load<Texture2D>(
					$"res://games/viljar_bike_escape/assets/bybane/bybane_{n:D2}.png");
			}
			if (BybanePath != null)
				FindNearestBybaneWaypoint();
		}

		var mapSprite = GetParent().GetParent().GetNodeOrNull<Sprite2D>("MapSprite");
		if (mapSprite?.Texture != null)
		{
			_mapW = mapSprite.Texture.GetWidth();
			_mapH = mapSprite.Texture.GetHeight();
			var tex = ResourceLoader.Load<Texture2D>("res://games/viljar_bike_escape/assets/map.png");
			if (tex != null)
				_mapImage = tex.GetImage();
		}

		Portrait = LoadRandomPortrait() ?? Portrait;
	}

	private Texture2D LoadRandomPortrait()
	{
		string folder = $"res://games/viljar_bike_escape/assets/portraits/{CharacterName.ToLower()}";
		var dir = DirAccess.Open(folder);
		if (dir == null) return null;

		var files = new Godot.Collections.Array<string>();
		dir.ListDirBegin();
		for (string f = dir.GetNext(); f != ""; f = dir.GetNext())
			if (!dir.CurrentIsDir() && (f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".jpeg") || f.EndsWith(".webp")))
				files.Add($"{folder}/{f}");
		dir.ListDirEnd();

		if (files.Count == 0) return null;
		return ResourceLoader.Load<Texture2D>(files[(int)GD.RandRange(0, files.Count - 1)]);
	}

	public void ActivateBugslide()
	{
		if (_bsState != BugslideState.None || _delay > 0f) return;

		_bsState       = BugslideState.Jump;
		_bsTimer       = JumpDuration;
		_jumpTravelDir = Vector2.Up.Rotated(Rotation); // keep going this way during the jump
		_jumpRotStart  = Rotation;
		_jumpRotTarget = Rotation + Mathf.Pi / 2f;     // rotate 90° in the air

		if (_collisionShape != null) _collisionShape.Disabled = true;
		if (_sprite != null) _sprite.Scale = _baseScale * BugslideScale;
	}

	public void ActivateThrow()
	{
		if (!ThrowReady || _robin == null) return;
		_throwTimer = ThrowCooldown;

		var disc = new Disc();
		GetParent().GetParent().AddChild(disc);
		disc.GlobalPosition = GlobalPosition;
		disc.Init((_robin.GlobalPosition - GlobalPosition).Normalized(), _robin);
	}

	public void ActivateRiceThrow()
	{
		if (_riceTimer > 0f) return;
		_riceTimer = RiceCooldown;
		ShowSpeechBubble("Smaker du risen!?");
		_game?.ShowDialogue(CharacterName, Portrait, "Smaker du risen!?");

		var target = FindNearestChaser();
		if (target == null) return;

		var rice = new Rice();
		GetParent().GetParent().AddChild(rice);
		rice.GlobalPosition = GlobalPosition;
		rice.Init((target.GlobalPosition - GlobalPosition).Normalized(), this, target);
	}

	private void ShootFigmaIcon()
	{
		if (_robin == null) return;
		string[] lines = { "Auto layout this!", "Not pixel perfect!", "Check your alignment, Robin!" };
		string line = lines[GD.RandRange(0, lines.Length - 1)];
		ShowSpeechBubble(line);

		var icon = new FigmaIcon();
		GetParent().GetParent().AddChild(icon);
		icon.GlobalPosition = GlobalPosition;
		icon.Init(_robin);
	}

	public void OnRiceHit(Chaser target)
	{
		string[] giLines = {
			"You've got a GI - Spike!",
			"Feel that blood sugar rush!",
			"Rice power activated!",
		};
		string line = giLines[GD.RandRange(0, giLines.Length - 1)];
		_game?.ShowDialogue(CharacterName, Portrait, line);
		ShowSpeechBubble(line);
	}

	private Chaser FindNearestChaser()
	{
		Chaser nearest  = null;
		float  bestDist = float.MaxValue;
		foreach (Node sibling in GetParent().GetChildren())
		{
			if (sibling == this || sibling is not Chaser other) continue;
			if (other.State == ChaseState.InCar || other.State == ChaseState.WaitingAtHome) continue;
			float d = GlobalPosition.DistanceTo(other.GlobalPosition);
			if (d < bestDist) { bestDist = d; nearest = other; }
		}
		return nearest;
	}

	public override void _Process(double delta)
	{
		QueueRedraw();
		if (_sprite != null)
			_sprite.Visible = State != ChaseState.InCar;
		UpdateSpriteScale();
		UpdateRunAnimation(delta);

		if (_spinTimer        > 0f) _spinTimer        -= (float)delta;
		if (_throwTimer       > 0f) _throwTimer       -= (float)delta;
		if (_slowTimer        > 0f) _slowTimer        -= (float)delta;
		if (_energyTimer      > 0f) _energyTimer      -= (float)delta;
		if (_giSpikeTimer     > 0f) _giSpikeTimer     -= (float)delta;
		if (_wineDrunkTimer   > 0f) _wineDrunkTimer   -= (float)delta;
		if (_wineCooldownTimer > 0f) _wineCooldownTimer -= (float)delta;
		if (_wineAoeTimer     > 0f) _wineAoeTimer     -= (float)delta;
		if (_bubbleTimer      > 0f) _bubbleTimer      -= (float)delta;

		// Drunk: every 3s pick a new angle within ±30° cone toward Robin and commit to it
		if (_wineDrunkTimer > 0f && _robin != null)
		{
			_drunkPhaseTimer -= (float)delta;
			if (_drunkPhaseTimer <= 0f)
			{
				float toRobin    = (_robin.GlobalPosition - GlobalPosition).Angle();
				_drunkFixedAngle = toRobin + (float)GD.RandRange(-DrunkConeHalf, DrunkConeHalf);
				_drunkPhaseTimer = DrunkPhaseDuration;
			}
		}
		else
		{
			_drunkPhaseTimer = 0f;
		}

		if (_homeWaitTimer > 0f)
		{
			_homeWaitTimer -= (float)delta;
			if (_homeWaitTimer <= 0f)
				State = ChaseState.Normal;
		}

		if (_delay > 0f || _robin == null) return;

		_dialogueTimer -= (float)delta;
		if (_dialogueTimer <= 0f)
		{
			TriggerDialogue();
			_dialogueTimer = (float)GD.RandRange(CooldownMin, CooldownMax);
		}

		if (_riceTimer > 0f) _riceTimer -= (float)delta;

		if (HasFigmaShot && _robin != null)
		{
			if (_figmaShotTimer > 0f)
				_figmaShotTimer -= (float)delta;
			else
			{
				ShootFigmaIcon();
				_figmaShotTimer = FigmaShotCooldown;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_robin == null) return;

		if (State == ChaseState.InCar)
		{
			if (_driver != null) GlobalPosition = _driver.GlobalPosition;
			return;
		}

		if (State == ChaseState.DrivingHome)
		{
			TickDrivingHome((float)delta);
			return;
		}

		if (State == ChaseState.WaitingAtHome)
		{
			Velocity = Vector2.Zero;
			return;
		}

		if (_spinTimer > 0f)
		{
			Rotation  += SpinRate * (float)delta;
			Velocity   = Vector2.Zero;
			MoveAndSlide();
			return;
		}

		_delay -= (float)delta;
		if (_delay > 0f) return;

		if (_bsState == BugslideState.Jump)
		{
			TickJump(delta);
			return;
		}

		if (_bsState == BugslideState.Slide)
		{
			TickSlide(delta);
			// Car facing is locked at 90° during slide — don't arc-steer
		}
		else if (!IsTent && !IsBybane)
		{
			float targetAngle;
			if (_wineDrunkTimer > 0f)
				targetAngle = _drunkFixedAngle + Mathf.Pi / 2f;
			else if (HasOrbit)
			{
				_orbitAngle += OrbitSpeed * (float)delta;
				var orbitTarget = _robin.GlobalPosition
					+ new Vector2(Mathf.Cos(_orbitAngle), Mathf.Sin(_orbitAngle)) * OrbitRadius;
				targetAngle = (orbitTarget - GlobalPosition).Angle() + Mathf.Pi / 2f;
			}
			else
				targetAngle = (_robin.GlobalPosition - GlobalPosition).Angle() + Mathf.Pi / 2f;

			float diff = Mathf.Wrap(targetAngle - Rotation, -Mathf.Pi, Mathf.Pi);
			Rotation += Mathf.Clamp(diff, -TurnSpeed * (float)delta, TurnSpeed * (float)delta);
		}
		// bybane: no rotation, direction handled at velocity assignment below

		// Separation
		var separation = Vector2.Zero;
		foreach (Node sibling in GetParent().GetChildren())
		{
			if (sibling == this) continue;
			if (sibling is not CharacterBody2D other) continue;
			var nudge = GlobalPosition - other.GlobalPosition;
			float dist = nudge.Length();
			if (dist < 60f && dist > 0f)
				separation += nudge.Normalized() * (60f - dist);
		}

		float effectiveSpeed = _bsState == BugslideState.Slide ? Speed * BugslideSpeedMult : Speed;
		if (_slowTimer       > 0f) effectiveSpeed *= SlowMult;
		if (_energyTimer     > 0f) effectiveSpeed *= EnergyDrinkSpeedMult;
		if (_giSpikeTimer    > 0f) effectiveSpeed *= GiSpikeMult;
		if (_wineDrunkTimer  > 0f) effectiveSpeed *= DrunkSpeedMult;
		// During slide: travel in _slideAngle direction (not car facing)
		var moveDir = _bsState == BugslideState.Slide
			? Vector2.Up.Rotated(_slideAngle)
			: Vector2.Up.Rotated(Rotation);
		if (_mapImage != null)
		{
			bool onWater = SampleIsWaterAt(GlobalPosition);
			if (IsVehicle)
			{
				var proposed = GlobalPosition + Vector2.Up.Rotated(Rotation) * effectiveSpeed * (float)delta;
				if (SampleIsWaterAt(proposed))
				{
					CancelSlide();
					Velocity = Vector2.Zero;
					MoveAndSlide();
					return;
				}
			}
			else if (onWater)
			{
				effectiveSpeed *= WaterSpeedMult;
				if (!_wasOnWater && WaterEntryLine != "")
				{
					_game?.ShowDialogue(CharacterName, Portrait, WaterEntryLine);
					ShowSpeechBubble(WaterEntryLine);
					if (WaterEntryVideo != "")
						_game?.PlayVideo(WaterEntryVideo);
				}
			}
			_wasOnWater = onWater;
		}

		if (IsBybane)
		{
			float dt = (float)delta;
			Vector2 toTarget;

			if (BybanePath != null)
			{
				var pts = BybanePath.Curve.GetBakedPoints();
				if (pts.Length > 1)
				{
					var worldTarget = BybanePath.ToGlobal(pts[_bybaneWpIndex]);
					if (GlobalPosition.DistanceTo(worldTarget) < BybaneWpReachDist)
					{
						_bybaneWpIndex += _bybaneWpDir;
						if (_bybaneWpIndex >= pts.Length) { _bybaneWpIndex = pts.Length - 2; _bybaneWpDir = -1; }
						if (_bybaneWpIndex < 0)           { _bybaneWpIndex = 1;              _bybaneWpDir =  1; }
						worldTarget = BybanePath.ToGlobal(pts[_bybaneWpIndex]);
					}
					toTarget = (worldTarget - GlobalPosition).Normalized();
				}
				else
				{
					toTarget = Vector2.Zero;
				}
			}
			else
			{
				// Fallback: chase Robin directly if no path is assigned
				toTarget = (_robin.GlobalPosition - GlobalPosition).Normalized();
			}

			float alignment = Velocity.LengthSquared() > 1f ? Velocity.Normalized().Dot(toTarget) : 1f;
			if (alignment > 0.92f)
				_bybaneSpeed = Mathf.Min(_bybaneSpeed + BybaneAccel * dt, BybaneMaxSpeed);
			else
				_bybaneSpeed = Mathf.Max(_bybaneSpeed - BybaneDecel * (1f - alignment) * dt, BybaneMinSpeed);

			if (_slowTimer > 0f) _bybaneSpeed *= SlowMult;

			Velocity = toTarget * _bybaneSpeed + separation * 2f;
		}
		else
		{
			Velocity = moveDir * effectiveSpeed + separation * 2f;
		}
		MoveAndSlide();
	}

	private void TickJump(double delta)
	{
		_bsTimer -= (float)delta;
		float t  = 1f - Mathf.Clamp(_bsTimer / JumpDuration, 0f, 1f);

		// Smoothly rotate 90° during the air phase
		Rotation = Mathf.LerpAngle(_jumpRotStart, _jumpRotTarget, t);

		// Keep travelling in the original direction — the car is airborne
		Velocity = _jumpTravelDir * Speed;
		MoveAndSlide();

		if (_bsTimer <= 0f)
		{
			// Land — car facing is now 90° rotated, travel direction is still original
			Rotation    = _jumpRotTarget;
			_slideAngle = _jumpRotStart; // velocity starts from original direction
			_bsState    = BugslideState.Slide;
			_bsTimer    = SlideDuration;
			if (_collisionShape != null) _collisionShape.Disabled = false;
		}
	}

	private void TickSlide(double delta)
	{
		_bsTimer -= (float)delta;

		// Travel direction lerps from original toward car facing (the 90° rotation) over SlideDuration
		float progress  = 1f - Mathf.Clamp(_bsTimer / SlideDuration, 0f, 1f);
		_slideAngle     = Mathf.LerpAngle(_jumpRotStart, _jumpRotTarget, progress);

		if (_bsTimer <= 0f)
			CancelSlide();
	}

	private void CancelSlide()
	{
		_bsState = BugslideState.None;
		if (_collisionShape != null) _collisionShape.Disabled = false;
		if (_sprite != null) _sprite.Scale = _baseScale;
	}

	private void UpdateSpriteScale()
	{
		if (_sprite == null) return;
		if (_bsState == BugslideState.Jump)
		{
			_sprite.Scale = _baseScale * BugslideScale; // stays big while airborne
		}
		else if (_bsState == BugslideState.Slide)
		{
			float t = _bsTimer / SlideDuration;
			_sprite.Scale = _baseScale * (1f + (BugslideScale - 1f) * t); // shrinks as slide drains
		}
	}

	public override void _Draw()
	{
		if (IsBybane && _bybaneFrames != null)
		{
			var tex = _bybaneFrames[GetBybaneFrameIndex()];
			if (tex != null)
			{
				var size = new Vector2(tex.GetWidth(), tex.GetHeight());
				DrawTexture(tex, -size / 2f);
			}
		}
		else if (_sprite == null)
		{
			if (IsTent) DrawTent();
			else if (IsVehicle) DrawCar();
			else DrawPerson();
		}

		if (_slowTimer > 0f)
		{
			float pulse = 0.5f + 0.5f * Mathf.Sin(_slowTimer * 8f);
			DrawArc(Vector2.Zero, 20f, 0f, Mathf.Tau, 32, new Color(0.3f, 0.5f, 1f, 0.55f + 0.35f * pulse), 3f);
		}

		if (_energyTimer > 0f)
		{
			float pulse = 0.5f + 0.5f * Mathf.Sin(_energyTimer * 15f);
			DrawArc(Vector2.Zero, 24f, 0f, Mathf.Tau, 32, new Color(0.1f, 1f, 0.3f, 0.45f + 0.4f * pulse), 3f);
		}

		if (_giSpikeTimer > 0f)
		{
			float pulse = 0.5f + 0.5f * Mathf.Sin(_giSpikeTimer * 12f);
			DrawArc(Vector2.Zero, 26f, 0f, Mathf.Tau, 32, new Color(1f, 0.6f, 0.1f, 0.45f + 0.4f * pulse), 3f);
			DrawGiSpikeChars();
		}

		if (_wineDrunkTimer > 0f)
		{
			float pulse = 0.5f + 0.5f * Mathf.Sin(_wineDrunkTimer * 7f);
			DrawArc(Vector2.Zero, 22f, 0f, Mathf.Tau, 32, new Color(0.6f, 0.05f, 0.15f, 0.4f + 0.3f * pulse), 3f);
			DrawWineGlass();
		}

		if (_wineAoeTimer > 0f)
		{
			float t      = 1f - _wineAoeTimer / WineAoeDuration;
			float radius = t * WineAoeRadius;
			float alpha  = (1f - t) * 0.55f;
			DrawArc(Vector2.Zero, radius, 0f, Mathf.Tau, 48, new Color(0.7f, 0.05f, 0.1f, alpha), 4f);
		}

		DrawNameLabel();
		DrawSpeechBubble();
	}

	private void DrawGiSpikeChars()
	{
		var   font    = ThemeDB.FallbackFont;
		var   chars   = new[] { "米", "氣", "↑", "血糖!", "能量" };
		float elapsed = _giSpikeTotalDuration - _giSpikeTimer; // 0 → total

		for (int i = 0; i < 3; i++)
		{
			float phase  = (elapsed * 1.2f + i * 0.7f) % 2.2f; // each char cycles over ~2s
			float yOff   = -55f - phase * 20f;                   // floats upward
			float xOff   = (i - 1) * 20f;
			float alpha  = Mathf.Clamp(1.4f - phase / 2.2f * 1.4f, 0f, 1f);

			string ch    = chars[(i + (int)(elapsed * 1.5f)) % chars.Length];
			var localPos = new Vector2(xOff, yOff).Rotated(-Rotation);
			DrawSetTransform(localPos, -Rotation, Vector2.One);
			DrawString(font, new Vector2(-9f, 0f), ch, HorizontalAlignment.Left, -1, 14,
				new Color(1f, 0.85f, 0.1f, alpha));
			DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
		}
	}

	private void DrawWineGlass()
	{
		// Small wine glass drawn above and to the right, counter-rotated
		var localPos = new Vector2(18f, -44f).Rotated(-Rotation);
		DrawSetTransform(localPos, -Rotation, Vector2.One);

		var wine   = new Color(0.55f, 0.04f, 0.12f, 0.95f);
		var glass  = new Color(0.88f, 0.92f, 0.96f, 0.75f);
		var border = new Color(0.25f, 0.1f, 0.15f, 0.9f);

		// Bowl (trapezoid, wider at top)
		DrawColoredPolygon(new Vector2[] {
			new Vector2(-7f, -9f), new Vector2(7f, -9f),
			new Vector2(4f,  3f),  new Vector2(-4f, 3f),
		}, glass);
		// Wine fill (bottom half of bowl)
		DrawColoredPolygon(new Vector2[] {
			new Vector2(-5f, -1f), new Vector2(5f, -1f),
			new Vector2(4f,  3f),  new Vector2(-4f, 3f),
		}, wine);
		// Stem
		DrawLine(new Vector2(0f, 3f), new Vector2(0f, 10f), border, 1.5f);
		// Base
		DrawLine(new Vector2(-5f, 10f), new Vector2(5f, 10f), border, 2f);

		DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
	}

	private int GetBybaneFrameIndex()
	{
		if (Velocity.LengthSquared() < 1f) return 0;
		float deg = Mathf.RadToDeg(Velocity.Angle());
		if (deg < 0f) deg += 360f;
		return ((int)((deg + BybaneFrameOffset) / 18f) % 20 + 20) % 20;
	}

	private void DrawCar()
	{
		var silver    = new Color(0.85f, 0.87f, 0.90f);
		var darkGray  = new Color(0.25f, 0.27f, 0.30f);
		var glass     = new Color(0.55f, 0.72f, 0.92f, 0.85f);
		var wheel     = new Color(0.12f, 0.12f, 0.12f);
		var headlight = new Color(1.0f,  1.0f,  0.80f);

		DrawRect(new Rect2(-11f, -18f, 22f, 36f), silver);
		DrawRect(new Rect2(-8f,  -10f, 16f, 18f), darkGray);
		DrawRect(new Rect2(-7f,  -17f, 14f,  8f), glass);
		DrawRect(new Rect2(-6f,    8f, 12f,  6f), glass);
		DrawRect(new Rect2(-15f, -16f,  5f,  9f), wheel);
		DrawRect(new Rect2( 10f, -16f,  5f,  9f), wheel);
		DrawRect(new Rect2(-15f,   7f,  5f,  9f), wheel);
		DrawRect(new Rect2( 10f,   7f,  5f,  9f), wheel);
		DrawRect(new Rect2(-10f, -20f,  6f,  3f), headlight);
		DrawRect(new Rect2(  4f, -20f,  6f,  3f), headlight);
	}

	private void UpdateRunAnimation(double delta)
	{
		if (_sprite == null || AltFrame == null || _baseFrame == null) return;
		if (_bsState != BugslideState.None) return; // bugslide handles its own scale

		bool moving = Velocity.LengthSquared() > 200f;
		if (moving)
		{
			_animTimer += (float)delta;
			if (_animTimer >= 1f / AnimFps)
			{
				_animTimer  = 0f;
				_animToggle = !_animToggle;
				_sprite.Texture = _animToggle ? AltFrame : _baseFrame;
			}
		}
		else
		{
			_sprite.Texture = _baseFrame;
			_animTimer = 0f;
		}
	}

	private void DrawTent()
	{
		var canvas  = new Color(0.85f, 0.50f, 0.15f); // orange tent
		var ridge   = new Color(0.50f, 0.28f, 0.06f); // dark ridge
		var rope    = new Color(0.65f, 0.60f, 0.45f);
		var peg     = new Color(0.22f, 0.18f, 0.12f);

		// Tent footprint — oval polygon
		int  n   = 14;
		var  pts = new Vector2[n];
		for (int i = 0; i < n; i++)
		{
			float a = Mathf.Tau * i / n;
			pts[i]  = new Vector2(Mathf.Cos(a) * 13f, Mathf.Sin(a) * 19f);
		}
		DrawColoredPolygon(pts, canvas);

		// Ridge shadow strip
		DrawColoredPolygon(new Vector2[] {
			new Vector2(-3f, -19f), new Vector2(3f, -19f),
			new Vector2(3f,  19f),  new Vector2(-3f, 19f)
		}, ridge);

		// Ridge line
		DrawLine(new Vector2(0f, -19f), new Vector2(0f, 19f), new Color(0.3f, 0.12f, 0f), 2f);

		// Guy ropes
		DrawLine(new Vector2(0f, -19f), new Vector2(-18f, -30f), rope, 1f);
		DrawLine(new Vector2(0f, -19f), new Vector2( 18f, -30f), rope, 1f);
		DrawLine(new Vector2(0f,  19f), new Vector2(-18f,  30f), rope, 1f);
		DrawLine(new Vector2(0f,  19f), new Vector2( 18f,  30f), rope, 1f);

		// Pegs
		DrawCircle(new Vector2(-18f, -30f), 3f, peg);
		DrawCircle(new Vector2( 18f, -30f), 3f, peg);
		DrawCircle(new Vector2(-18f,  30f), 3f, peg);
		DrawCircle(new Vector2( 18f,  30f), 3f, peg);

		// Tent entrance (small arc at front)
		DrawArc(new Vector2(0f, -17f), 5f, Mathf.Pi * 1.1f, Mathf.Pi * 1.9f, 10, peg, 2f);
	}

	private void DrawPerson()
	{
		DrawCircle(Vector2.Zero, 12f, new Color(1f, 0.5f, 0f));
		DrawString(ThemeDB.FallbackFont, new Vector2(-5f, 5f), CharacterName[..1], HorizontalAlignment.Left, -1, 12);
	}

	private bool SampleIsWaterAt(Vector2 worldPos)
	{
		int px = Mathf.Clamp((int)(worldPos.X + _mapW / 2f), 0, _mapW - 1);
		int py = Mathf.Clamp((int)(worldPos.Y + _mapH / 2f), 0, _mapH - 1);
		Color c = _mapImage.GetPixel(px, py);
		return Mathf.Abs(c.R - WaterColor.R) < ColorTolerance
			&& Mathf.Abs(c.G - WaterColor.G) < ColorTolerance
			&& Mathf.Abs(c.B - WaterColor.B) < ColorTolerance;
	}

	private void DrawNameLabel()
	{
		var font = ThemeDB.FallbackFont;

		// Always draw above the character in screen space, regardless of node rotation
		var localPos = new Vector2(0f, -32f).Rotated(-Rotation);
		DrawSetTransform(localPos, -Rotation, Vector2.One);

		// Dark backing so text is readable over any map colour
		var textSize = font.GetStringSize(CharacterName, HorizontalAlignment.Left, -1, 11);
		DrawRect(new Rect2(-textSize.X / 2f - 2f, -11f, textSize.X + 4f, 13f), new Color(0f, 0f, 0f, 0.55f));
		DrawString(font, new Vector2(-textSize.X / 2f, 0f), CharacterName,
				   HorizontalAlignment.Left, -1, 11, Colors.White);

		DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
	}

	private void DrawSpeechBubble()
	{
		if (_bubbleTimer <= 0f || _bubbleText == null) return;

		float alpha   = _bubbleTimer < SpeechBubbleFadeTime
			? _bubbleTimer / SpeechBubbleFadeTime
			: 1f;

		var  font     = ThemeDB.FallbackFont;
		int  fontSize = 11;
		const float pad    = 6f;
		const float maxW   = 130f;
		const float tailH  = 7f;

		var textSize  = font.GetStringSize(_bubbleText, HorizontalAlignment.Left, maxW, fontSize);
		float bw      = Mathf.Min(textSize.X + pad * 2f, maxW + pad * 2f);
		float bh      = textSize.Y + pad * 2f;

		// Counter-rotate, sit above the name label
		var localPos  = new Vector2(0f, -52f).Rotated(-Rotation);
		DrawSetTransform(localPos, -Rotation, Vector2.One);

		// Bubble body
		var bg     = new Color(1f, 1f, 0.92f, 0.95f * alpha);
		var border = new Color(0.15f, 0.15f, 0.15f, alpha);
		DrawRect(new Rect2(-bw / 2f, -bh - tailH, bw, bh), bg);
		DrawRect(new Rect2(-bw / 2f, -bh - tailH, bw, bh), border, false, 1.5f);

		// Tail pointing down
		DrawColoredPolygon(new Vector2[]
		{
			new Vector2(-6f, -tailH),
			new Vector2( 6f, -tailH),
			new Vector2( 0f,  0f)
		}, bg);
		DrawLine(new Vector2(-6f, -tailH), new Vector2(0f, 0f), border, 1.5f);
		DrawLine(new Vector2( 6f, -tailH), new Vector2(0f, 0f), border, 1.5f);

		// Text
		DrawString(font, new Vector2(-bw / 2f + pad, -tailH - pad),
			_bubbleText, HorizontalAlignment.Left, maxW, fontSize,
			new Color(0.1f, 0.1f, 0.1f, alpha));

		DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
	}

	public void ShowSpeechBubble(string text)
	{
		_bubbleText  = text;
		_bubbleTimer = SpeechBubbleDuration;
	}

	private void TriggerDialogue()
	{
		if (_game == null || Lines.Length == 0) return;
		string line = Lines[GD.RandRange(0, Lines.Length - 1)];
		_game.ShowDialogue(CharacterName, Portrait, line);
		ShowSpeechBubble(line);
	}

	public void PickUpPassenger(Chaser viljar)
	{
		viljar._driver = this;
		viljar.State   = ChaseState.InCar;
		viljar.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
		CarryingPassenger = viljar;
		State             = ChaseState.DrivingHome;
		FindNearestWaypoint();
	}

	private void ArriveHome()
	{
		var dropPos = _game?.HomePosition ?? GlobalPosition;
		CarryingPassenger.GlobalPosition = dropPos;
		CarryingPassenger.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
		CarryingPassenger._driver        = null;
		CarryingPassenger.State          = ChaseState.WaitingAtHome;
		CarryingPassenger._homeWaitTimer = HomeWaitDuration;
		CarryingPassenger = null;

		State = ChaseState.Normal;
	}

	private void TickDrivingHome(float delta)
	{
		Vector2 target;

		if (HomePath != null)
		{
			var pts = HomePath.Curve.GetBakedPoints();
			if (pts.Length > 0)
			{
				var worldPt = HomePath.ToGlobal(pts[_waypointIndex]);
				if (_waypointIndex < pts.Length - 1 && GlobalPosition.DistanceTo(worldPt) < WaypointReachDist)
					_waypointIndex++;
				target = HomePath.ToGlobal(pts[_waypointIndex]);
			}
			else
			{
				target = _game?.HomePosition ?? GlobalPosition;
			}
		}
		else
		{
			target = _game?.HomePosition ?? GlobalPosition;
		}

		Rotation = (target - GlobalPosition).Angle() + Mathf.Pi / 2f;

		Velocity = Vector2.Up.Rotated(Rotation) * Speed;
		MoveAndSlide();

		var homePos = _game?.HomePosition ?? Vector2.Zero;
		if (GlobalPosition.DistanceTo(homePos) < HomeArrivalDist)
			ArriveHome();
	}

	private void FindNearestWaypoint()
	{
		if (HomePath == null) { _waypointIndex = 0; return; }
		var pts  = HomePath.Curve.GetBakedPoints();
		float best = float.MaxValue;
		_waypointIndex = 0;
		for (int i = 0; i < pts.Length; i++)
		{
			float d = GlobalPosition.DistanceTo(HomePath.ToGlobal(pts[i]));
			if (d >= best) continue;
			best           = d;
			_waypointIndex = i;
		}
	}

	private void FindNearestBybaneWaypoint()
	{
		if (BybanePath == null) { _bybaneWpIndex = 0; return; }
		var pts = BybanePath.Curve.GetBakedPoints();
		float best = float.MaxValue;
		_bybaneWpIndex = 0;
		for (int i = 0; i < pts.Length; i++)
		{
			float d = GlobalPosition.DistanceTo(BybanePath.ToGlobal(pts[i]));
			if (d >= best) continue;
			best           = d;
			_bybaneWpIndex = i;
		}
		// Start going forward from wherever we snap to
		_bybaneWpDir = _bybaneWpIndex < pts.Length - 1 ? 1 : -1;
	}
}
