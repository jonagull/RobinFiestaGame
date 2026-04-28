using Godot;

public partial class Robin : CharacterBody2D
{
	[Export] public Texture2D AltFrame = null;

	private const float BikeSpeed   = 220f;
	private const float CanoeSpeed  = 130f;
	private const float TurnSpeed   = 3.0f;
	private const float AnimFps     = 8f;

	// OSM standard tile water colour ≈ #aad3df
	private static readonly Color WaterColor    = new Color(0.667f, 0.827f, 0.875f);
	private const           float ColorTolerance = 0.12f;

	public bool IsOnWater { get; private set; } = false;

	private const float HitSlowDuration  = 2.5f;
	private const float HitSlowMult      = 0.35f;
	private float       _hitSlowTimer    = 0f;

	private const float FigmaSlowDuration = 3f;
	private const float FigmaSlowMult     = 0.45f;
	private float       _figmaSlowTimer   = 0f;

	public void ApplyDiscHit()   => _hitSlowTimer   = HitSlowDuration;
	public void ApplyFigmaSlow() => _figmaSlowTimer = FigmaSlowDuration;
	public bool IsHit            => _hitSlowTimer   > 0f;
	public bool IsFigmaSlowed    => _figmaSlowTimer > 0f;

	private Image     _mapImage;
	private int       _mapW;
	private int       _mapH;
	private Sprite2D  _sprite;
	private Texture2D _baseFrame;
	private float     _animTimer  = 0f;
	private bool      _animToggle = false;

	public override void _Ready()
	{
		_sprite    = GetNodeOrNull<Sprite2D>("Sprite2D");
		_baseFrame = _sprite?.Texture;

		var mapSprite = GetParent().GetNodeOrNull<Sprite2D>("MapSprite");
		if (mapSprite?.Texture != null)
		{
			_mapW = mapSprite.Texture.GetWidth();
			_mapH = mapSprite.Texture.GetHeight();

			var tex = ResourceLoader.Load<Texture2D>("res://games/viljar_bike_escape/assets/map.png");
			if (tex != null)
				_mapImage = tex.GetImage();
		}
	}

	public override void _Process(double delta)
	{
		QueueRedraw();

		if (_sprite != null && AltFrame != null && _baseFrame != null)
		{
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
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_mapImage != null)
			IsOnWater = SampleIsWater();

		if (_hitSlowTimer   > 0f) _hitSlowTimer   -= (float)delta;
		if (_figmaSlowTimer > 0f) _figmaSlowTimer -= (float)delta;

		float speed = IsOnWater ? CanoeSpeed : BikeSpeed;
		if (_hitSlowTimer   > 0f) speed *= HitSlowMult;
		if (_figmaSlowTimer > 0f) speed *= FigmaSlowMult;

		// Steering: left/right rotate, up moves forward
		float turn = Input.GetAxis("ui_left", "ui_right");
		Rotation += turn * TurnSpeed * (float)delta;

		float fwd = Input.GetActionStrength("ui_up");
		Velocity = Vector2.Up.Rotated(Rotation) * speed * fwd;
		MoveAndSlide();
	}

	public override void _Draw()
	{
		if (_sprite != null) return;
		Color fill = IsOnWater ? new Color(0.2f, 0.5f, 1f) : new Color(1f, 0.2f, 0.2f);
		DrawCircle(Vector2.Zero, 14f, fill);
		DrawString(ThemeDB.FallbackFont, new Vector2(-6f, 6f), "R", HorizontalAlignment.Left, -1, 14);
	}

	private bool SampleIsWater()
	{
		int px = Mathf.Clamp((int)(GlobalPosition.X + _mapW / 2f), 0, _mapW - 1);
		int py = Mathf.Clamp((int)(GlobalPosition.Y + _mapH / 2f), 0, _mapH - 1);

		Color c = _mapImage.GetPixel(px, py);
		return Mathf.Abs(c.R - WaterColor.R) < ColorTolerance
			&& Mathf.Abs(c.G - WaterColor.G) < ColorTolerance
			&& Mathf.Abs(c.B - WaterColor.B) < ColorTolerance;
	}
}
