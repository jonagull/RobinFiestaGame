using Godot;

public partial class ObjectiveArrow : Node2D
{
	public Vector2  GoalWorldPos;
	public Node2D   Robin;
	public Camera2D Camera;

	private const float Margin    = 48f;
	private const float ArrowSize = 16f;

	public override void _Process(double delta)
	{
		if (Robin == null) return;
		QueueRedraw();

		var vpSize       = GetViewport().GetVisibleRect().Size;
		var screenCenter = vpSize / 2f;
		var zoom         = Camera?.Zoom ?? Vector2.One;

		// World-space direction = screen-space direction (camera follows Robin)
		var worldDelta  = GoalWorldPos - Robin.GlobalPosition;
		if (worldDelta.LengthSquared() < 1f) { Visible = false; return; }

		var screenDelta = new Vector2(worldDelta.X * zoom.X, worldDelta.Y * zoom.Y);

		// Hide when goal is within the safe inner area of the screen
		var goalScreen = screenCenter + screenDelta;
		bool onScreen  = goalScreen.X > Margin && goalScreen.X < vpSize.X - Margin
		              && goalScreen.Y > Margin && goalScreen.Y < vpSize.Y - Margin;
		Visible = !onScreen;
		if (onScreen) return;

		// Find where the direction ray hits the screen edge (inset by Margin)
		var dir = screenDelta.Normalized();
		float hw = vpSize.X / 2f - Margin;
		float hh = vpSize.Y / 2f - Margin;
		float tx = dir.X != 0f ? hw / Mathf.Abs(dir.X) : float.MaxValue;
		float ty = dir.Y != 0f ? hh / Mathf.Abs(dir.Y) : float.MaxValue;

		Position = screenCenter + dir * Mathf.Min(tx, ty);
		Rotation = dir.Angle() + Mathf.Pi / 2f;
	}

	public override void _Draw()
	{
		var fill   = new Color(0.2f, 1f, 0.35f, 0.92f);
		var border = new Color(0f, 0f, 0f, 0.5f);

		// Arrowhead
		DrawColoredPolygon(new Vector2[]
		{
			new Vector2(0f,                  -ArrowSize),
			new Vector2(-ArrowSize * 0.65f,   ArrowSize * 0.2f),
			new Vector2( ArrowSize * 0.65f,   ArrowSize * 0.2f),
		}, fill);
		DrawPolyline(new Vector2[]
		{
			new Vector2(0f,                  -ArrowSize),
			new Vector2(-ArrowSize * 0.65f,   ArrowSize * 0.2f),
			new Vector2( ArrowSize * 0.65f,   ArrowSize * 0.2f),
			new Vector2(0f,                  -ArrowSize),
		}, border, 1.5f);

		// Shaft
		DrawRect(new Rect2(-ArrowSize * 0.2f, ArrowSize * 0.2f, ArrowSize * 0.4f, ArrowSize * 0.5f), fill);
		DrawRect(new Rect2(-ArrowSize * 0.2f, ArrowSize * 0.2f, ArrowSize * 0.4f, ArrowSize * 0.5f), border, false, 1f);
	}
}
