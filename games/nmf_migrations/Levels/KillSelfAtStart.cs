using Godot;

public partial class KillSelfAtStart : Sprite2D
{
    public override void _EnterTree()
    {
        QueueFree();
    }
}
