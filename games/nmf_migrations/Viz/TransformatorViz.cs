using System;
using Godot;
using Sim;

namespace RobinFiesta.games.nmf_migrations.Viz;

public partial class TransformatorViz : Node2D
{
    private Machine.Machine _machine;

    [Export]
    private string _transformation = "";

    private Area2D _inputArea;

    private const string Group = "ShapeGroup";

    public override void _EnterTree()
    {
        var trans = _transformation.ToLower() switch
        {
            "l" => Shapes.Transformation.RotateLeft,
            "r" => Shapes.Transformation.RotateRight,
            "h" => Shapes.Transformation.FlipHorizontal,
            "v" => Shapes.Transformation.FlipVertical,
            _ => throw new ArgumentOutOfRangeException()
        };
        _machine = new Transformator.Transformator(trans);
    }

    public override void _Ready()
    {
        _inputArea = GetNodeOrNull<Area2D>("InputArea") 
                     ?? throw new NullReferenceException("Missing InputArea");
        _inputArea.AreaEntered += area =>
        {
            GD.Print("Enter");
            if (!area.GetGroups().Contains(Group)) return;
            GD.Print("!!!");
            var shapeViz = area.GetParentOrNull<ShapeViz>()
                           ?? throw new NullReferenceException($"Expected Area2D parent to be ShapeViz since its in group: {Group}");
            RotateShape(shapeViz);
        };
        
        _inputArea.AreaExited += area =>
        {
            GD.Print("EXIT");
            if (!area.GetGroups().Contains(Group)) return;
            GD.Print("EXIT!!");
            var shapeViz = area.GetParentOrNull<ShapeViz>()
                           ?? throw new NullReferenceException($"Expected Area2D parent to be ShapeViz since its in group: {Group}");
            shapeViz.Show();
        };
    }

    private void RotateShape(ShapeViz shapeViz)
    {
        if (!shapeViz.Visible)
        {
            throw new Exception("ShapeViz not visible, but should be");
        }
        shapeViz.OnShapeChange();
        shapeViz.Hide();
        _machine.WorkShape(shapeViz.Shape);
    }
}