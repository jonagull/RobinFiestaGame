using System;
using Godot;
using Sim;

namespace RobinFiesta.games.nmf_migrations.Viz;

public partial class CutterViz : Node2D
{
    private Cutter.Cutter _machine;
    
    private readonly PackedScene _shapeViz =
        GD.Load<PackedScene>("res://games/nmf_migrations/Viz/shape.tscn");

    [Export]
    private string _cutDirection = "";

    private Area2D _inputArea;

    private const string Group = "ShapeGroup";

    public override void _EnterTree()
    {
        var trans = _cutDirection.ToLower() switch
        {
            "h" => Shapes.Cut.Horizontal,
            "v" => Shapes.Cut.Vertical,
            _ => throw new ArgumentOutOfRangeException()
        };
        _machine = new Cutter.Cutter(trans);
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
            CallDeferred("CutShape", shapeViz);
        };
        
        _inputArea.AreaExited += area =>
        {
            GD.Print("EXIT");
            if (!area.GetGroups().Contains(Group)) return;
            GD.Print("EXIT!!");
            var shapeViz = area.GetParentOrNull<ShapeViz>()
                           ?? throw new NullReferenceException($"Expected Area2D parent to be ShapeViz since its in group: {Group}");
            shapeViz.Show();
            shapeViz.CallDeferred("PostOnChangeShape");
        };
    }

    private void CutShape(ShapeViz shapeViz)
    {
        if (!shapeViz.Visible)
        {
            return;
        }
        var shape = _machine.CutShape(shapeViz.Shape);
        shapeViz.Hide();
        shapeViz.OnShapeChange();
        var shapeVizNode = _shapeViz.Instantiate<ShapeViz>();
        var tml = shapeVizNode.GetNodeOrNull<TileMapLayer>("FallBack");
        tml.Name = "TileMapLayer";
        shapeVizNode.Bricks = tml;
        shapeVizNode.SetShape(shape);
        var size = shapeVizNode.Bricks.GetUsedRect().Size;
        AddChild(shapeVizNode);
        shapeVizNode.GlobalPosition = shapeViz.GlobalPosition + size;
    }
}