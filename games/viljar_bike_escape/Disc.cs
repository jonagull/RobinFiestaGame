using Godot;

public partial class Disc : Node2D
{
	private Vector2 _velocity;
	private Vector2 _hookDir;    // fixed rightward direction from initial throw
	private Robin   _robin;
	private float   _lifetime;
	private float   _maxLifetime;

	private const float InitialSpeed = 480f;
	private const float MaxLifetime  = 3.2f;
	private const float DragCoeff    = 0.28f;  // gentle drag — disc keeps speed
	private const float HookAccel    = 650f;   // strong hook
	private const float HookStart    = 0.3f;   // fraction of flight before hook kicks in
	private const float HitRadius    = 16f;

	public void Init(Vector2 direction, Robin robin)
	{
		var dir      = direction.Normalized();
		_velocity    = dir * InitialSpeed;
		_hookDir     = dir.Rotated(Mathf.Pi / 2f); // fixed right-of-throw, never changes
		_robin       = robin;
		_lifetime    = MaxLifetime;
		_maxLifetime = MaxLifetime;
	}

	public override void _Process(double delta)
	{
		QueueRedraw();

		float dt  = (float)delta;
		float age = 1f - (_lifetime / _maxLifetime); // 0 → 1 over lifetime

		// Gentle drag
		_velocity *= 1f - DragCoeff * dt;

		// Hook — fixed rightward direction from initial throw, cubic ramp (slow start, hard finish)
		float hookT      = Mathf.Clamp((age - HookStart) / (1f - HookStart), 0f, 1f);
		float hookStrength = hookT * hookT * hookT;
		_velocity        += _hookDir * hookStrength * HookAccel * dt;

		Position  += _velocity * dt;
		_lifetime -= dt;

		if (_lifetime <= 0f || _velocity.LengthSquared() < 30f)
		{
			QueueFree();
			return;
		}

		if (_robin != null && GlobalPosition.DistanceTo(_robin.GlobalPosition) < HitRadius)
		{
			_robin.ApplyDiscHit();
			QueueFree();
		}
	}

	public override void _Draw()
	{
		// Spin increases over flight (opposite direction to before)
		float age  = 1f - (_lifetime / _maxLifetime);
		float spin = age * 18f;

		DrawCircle(new Vector2(2f, 2f), 8f, new Color(0f, 0f, 0f, 0.25f));
		DrawCircle(Vector2.Zero, 8f, new Color(0.95f, 0.95f, 0.92f));
		DrawArc(Vector2.Zero, 8f, 0f, Mathf.Tau, 24, new Color(0.55f, 0.55f, 0.50f), 1.5f);
		DrawLine(
			new Vector2(Mathf.Cos(spin) * 2f, Mathf.Sin(spin) * 2f),
			new Vector2(Mathf.Cos(spin) * 7f, Mathf.Sin(spin) * 7f),
			new Color(0.4f, 0.4f, 0.35f), 1.5f);
	}
}
