using Godot;

public partial class GolfBall : Node2D
{
	private const float HitRadius = 20f;
	private const float Lifetime  = 12f;
	private const float Friction  = 140f; // px/s² deceleration
	private float    _life     = Lifetime;
	private Vector2  _velocity;
	private Sprite2D _sprite;

	public void Init(Vector2 velocity) => _velocity = velocity;

	public override void _Ready()
	{
		_sprite = new Sprite2D();
		_sprite.Texture = ResourceLoader.Load<Texture2D>(
			"res://games/viljar_golf_game/assets/Dinky_Tiny_Golf_Free/Singles/Ball/Ball-Lay_0000_Sand.png");
		AddChild(_sprite);
	}

	public override void _Process(double delta)
	{
		_life -= (float)delta;
		if (_life <= 0f) { QueueFree(); return; }

		if (_sprite != null)
			_sprite.Modulate = new Color(1f, 1f, 1f, _life < 2f ? _life / 2f : 1f);

		if (_velocity.LengthSquared() > 1f)
		{
			Position  += _velocity * (float)delta;
			float spd  = Mathf.Max(0f, _velocity.Length() - Friction * (float)delta);
			_velocity  = _velocity.Normalized() * spd;
		}

		var chasers = GetParent()?.GetNodeOrNull<Node2D>("Chasers");
		if (chasers == null) return;

		foreach (Node child in chasers.GetChildren())
		{
			if (child is not Chaser chaser) continue;
			if (chaser.State == Chaser.ChaseState.InCar || chaser.State == Chaser.ChaseState.WaitingAtHome) continue;
			if (chaser.IsSpinning) continue;
			if (GlobalPosition.DistanceTo(chaser.GlobalPosition) < HitRadius)
			{
				chaser.ApplyGolfballSpin();
				QueueFree();
				return;
			}
		}
	}
}
