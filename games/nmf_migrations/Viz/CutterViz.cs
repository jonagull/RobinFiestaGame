using System;
using Godot;
using Sim;

namespace RobinFiesta.games.nmf_migrations.Viz;

public partial class CutterViz : Node2D
{
    private const string Group = "ShapeGroup";

    private readonly PackedScene _shapeViz =
        GD.Load<PackedScene>("res://games/nmf_migrations/Viz/shape.tscn");

    [Export] private string _cutDirection = "";

    private Area2D _inputArea;
    private Cutter.Cutter _machine;

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
            if (!area.GetGroups().Contains(Group)) return;
            var shapeViz = area.GetParentOrNull<ShapeViz>()
                           ?? throw new NullReferenceException(
                               $"Expected Area2D parent to be ShapeViz since its in group: {Group}");
            if (!shapeViz.IsCuttable) return;
            GD.Print($"Enter: {shapeViz.NativeInstance}");
            CallDeferred("CutShape", shapeViz);
        };

        _inputArea.AreaExited += area =>
        {
            if (!area.GetGroups().Contains(Group)) return;
            var shapeViz = area.GetParentOrNull<ShapeViz>()
                           ?? throw new NullReferenceException(
                               $"Expected Area2D parent to be ShapeViz since its in group: {Group}");
            if (!shapeViz.IsCuttable) return;
            GD.Print($"Exit: {shapeViz.NativeInstance}");
            shapeViz.Show();
            shapeViz.CallDeferred("PostOnChangeShape");
        };
    }

    private void CutShape(ShapeViz shapeViz)
    {
        if (!shapeViz.Visible) return;
        var shape = _machine.CutShape(shapeViz.Shape);
        shapeViz.Hide();
        shapeViz.OnShapeChange();
        var pos = GlobalPosition;
        var shapeVizNode = _shapeViz.Instantiate<ShapeViz>();
        shapeVizNode.IsCuttable = false;
        //pos.X -= shapeViz.Bricks.GetUsedRect().Size.X / 2f ;
        shapeVizNode.GlobalPosition = pos;
        //shapeViz.Debugging = true;
        //shapeVizNode.Debugging = true;
        GD.Print(shapeVizNode.Bricks);
        shapeVizNode.OnShapeChange();
        AddChild(shapeVizNode);
    }
}