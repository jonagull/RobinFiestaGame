using Godot;

public partial class EnergyDrink : Node2D
{
	[Export] public Texture2D CanTexture;

	private const float PickupRadius = 35f;
	private const float BobAmplitude = 3.5f;
	private const float BobSpeed     = 2.8f;

	private Node2D _chasers;
	private float  _bobTimer = 0f;

	public override void _Ready()
	{
		_chasers = GetParent().GetNodeOrNull<Node2D>("Chasers");
		QueueRedraw();
	}

	public override void _Process(double delta)
	{
		_bobTimer += (float)delta;
		QueueRedraw();

		if (_chasers == null) return;

		foreach (Node child in _chasers.GetChildren())
		{
			if (child is Chaser c && c.HasEnergyDrink &&
				c.GlobalPosition.DistanceTo(GlobalPosition) < PickupRadius)
			{
				c.ActivateEnergyDrink();
				QueueFree();
				return;
			}
		}
	}

	public override void _Draw()
	{
		float bob = Mathf.Sin(_bobTimer * BobSpeed) * BobAmplitude;
		var   off = new Vector2(0f, bob);

		if (CanTexture != null)
		{
			var drawSize = new Vector2(24f, 32f);
			var rect     = new Rect2(off - drawSize / 2f, drawSize);

			// Drop shadow
			DrawCircle(off + new Vector2(0f, drawSize.Y * 0.4f), drawSize.X * 0.45f,
				new Color(0f, 0f, 0f, 0.18f));

			// Glow ring
			float pulse = 0.5f + 0.5f * Mathf.Sin(_bobTimer * 5f);
			DrawCircle(off, drawSize.X * 0.85f, new Color(0.1f, 1f, 0.3f, 0.10f + 0.08f * pulse));

			DrawTextureRect(CanTexture, rect, false);
		}
		else
		{
			// Fallback drawn can if texture not set
			DrawRect(new Rect2(off.X - 7f, off.Y - 13f, 14f, 26f), new Color(0.05f, 0.50f, 0.12f));
		}
	}
}
