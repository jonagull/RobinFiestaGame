using Godot;

public partial class FigmaIcon : Node2D
{
	private Robin    _robin;
	private float    _lifetime = 4f;
	private float    _spin     = 0f;
	private Vector2  _velocity;
	private Sprite2D _sprite;

	private const float Speed     = 280f;
	private const float HitRadius = 18f;

	public override void _Ready()
	{
		_sprite         = new Sprite2D();
		_sprite.Texture = ResourceLoader.Load<Texture2D>(
			"res://games/viljar_bike_escape/assets/Figma-logo.svg.png");
		_sprite.Scale   = Vector2.One * 0.06f;
		AddChild(_sprite);
	}

	public void Init(Robin robin)
	{
		_robin    = robin;
		_velocity = (robin.GlobalPosition - GlobalPosition).Normalized() * Speed;
	}

	public override void _Process(double delta)
	{
		_lifetime -= (float)delta;
		_spin     += 3f * (float)delta;
		if (_sprite != null)
		{
			_sprite.Rotation = _spin;
			_sprite.Modulate = new Color(1f, 1f, 1f, _lifetime < 1f ? _lifetime : 1f);
		}
		if (_lifetime <= 0f) { QueueFree(); return; }

		Position += _velocity * (float)delta;

		if (_robin != null && IsInstanceValid(_robin)
			&& GlobalPosition.DistanceTo(_robin.GlobalPosition) < HitRadius)
		{
			_robin.ApplyFigmaSlow();
			QueueFree();
		}
	}

}
