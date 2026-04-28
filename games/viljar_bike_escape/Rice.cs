using Godot;

public partial class Rice : Node2D
{
	private Vector2 _velocity;
	private Chaser  _thrower;
	private Chaser  _target;
	private float   _lifetime;

	private const float InitialSpeed    = 350f;
	private const float MaxLifetime     = 2.5f;
	private const float HitRadius       = 28f;
	private const float HomingStrength  = 0.25f; // fraction per frame blended toward target
	public  const float GiSpikeDuration = 4f;

	public void Init(Vector2 direction, Chaser thrower, Chaser target)
	{
		_velocity = direction.Normalized() * InitialSpeed;
		_thrower  = thrower;
		_target   = target;
		_lifetime = MaxLifetime;
	}

	public override void _Process(double delta)
	{
		QueueRedraw();

		// Gentle homing — nudge velocity toward target's current position
		if (_target != null && IsInstanceValid(_target))
		{
			var toTarget = (_target.GlobalPosition - GlobalPosition).Normalized();
			_velocity    = (_velocity.Normalized() + toTarget * HomingStrength).Normalized() * _velocity.Length();
		}

		Position  += _velocity * (float)delta;
		_lifetime -= (float)delta;

		if (_lifetime <= 0f) { QueueFree(); return; }

		// Hit check against all chasers (in case of near-miss on another chaser)
		var chasersNode = GetParent()?.GetNodeOrNull<Node2D>("Chasers");
		if (chasersNode == null) { QueueFree(); return; }

		foreach (Node child in chasersNode.GetChildren())
		{
			if (child is not Chaser chaser || chaser == _thrower) continue;
			if (GlobalPosition.DistanceTo(chaser.GlobalPosition) < HitRadius)
			{
				chaser.ApplyGiSpike(GiSpikeDuration);
				_thrower?.OnRiceHit(chaser);
				QueueFree();
				return;
			}
		}
	}

	public override void _Draw()
	{
		// Six rice grains as small rotated white rectangles
		var grains = new (Vector2 pos, float rot)[]
		{
			(new Vector2(-7f, -3f),  0.3f),
			(new Vector2( 4f, -6f), -0.8f),
			(new Vector2( 0f,  4f),  1.2f),
			(new Vector2(-3f,  6f), -0.2f),
			(new Vector2( 7f,  2f),  0.9f),
			(new Vector2(-5f,  1f), -1.1f),
		};
		var color = new Color(1f, 0.97f, 0.88f);
		foreach (var (pos, rot) in grains)
		{
			DrawSetTransform(pos, rot, Vector2.One);
			DrawRect(new Rect2(-4f, -1.5f, 8f, 3f), color);
		}
		DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
	}
}
