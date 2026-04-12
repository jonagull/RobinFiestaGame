using Godot;
using System;

public partial class ScaleCollsion : Area2D
{
    [Export] private AnimatedSprite2D _sprite2D;
    private CollisionShape2D _collisionShape2D;
    public override void _Ready()
    {
        _collisionShape2D = GetNodeOrNull<CollisionShape2D>("CollisionShape2D")
                ?? throw new Exception("CollisionShape2D not found");
        var fIdx = _sprite2D.GetFrame();
        var anim = _sprite2D.GetAnimation();
        var targetSize = _sprite2D.SpriteFrames.GetFrameTexture(anim, fIdx).GetSize();
        var s = new RectangleShape2D();
        s.Size = targetSize;
        _collisionShape2D.SetShape(s);
    }
}
