using System;
using System.Collections.Generic;
using Godot;

public abstract partial class MachineViz : Node2D
{
    private const string Group = "ShapeGroup";

    private readonly List<ShapeViz> _inputShapes = [];

    protected readonly List<ShapeViz> OutputShapes = [];

    private ShapeViz.Direction _outputDirection;

    [Export] private string _outputDirectionRaw = "Down";

    private double _timePassed;

    private Area2D _workArea;

    [Export] private double _workTime = 2;

    protected void AddOutput(ShapeViz shapeViz)
    {
        OutputShapes.Add(shapeViz);
    }

    public override void _Ready()
    {
        _ = Enum.TryParse(_outputDirectionRaw, true, out _outputDirection)
            ? 0
            : throw new Exception($"Failed to parse: {_outputDirectionRaw}");

        _workArea = GetNodeOrNull<Area2D>("WorkArea")
                    ?? throw new NullReferenceException("Missing WorkArea");

        _workArea.AreaEntered += area =>
        {
            if (!area.GetGroups().Contains(Group)) return;
            var shapeViz = area.GetParentOrNull<ShapeViz>()
                           ?? throw new NullReferenceException(
                               $"Expected Area2D parent to be ShapeViz since its in group: {Group}");
            if (!shapeViz.Visible) return;
            _inputShapes.Add(shapeViz);
            shapeViz.Visible = false;
            shapeViz.CurrentDirection = ShapeViz.Direction.None;
        };
    }

    protected abstract void TransformShape(ShapeViz shapeViz);

    public override void _Process(double delta)
    {
        // We are only "working" if there are shapes to work on
        if (_inputShapes.Count == 0 && OutputShapes.Count == 0) return;
        _timePassed += delta;

        // We finished the work, time to pop the shape
        if (_timePassed < _workTime) return;
        _timePassed = 0; 

        if (_inputShapes.Count != 0)
        {
            var inputShape = _inputShapes[0];
            _inputShapes.RemoveAt(0);
            TransformShape(inputShape);
        }

        if (OutputShapes.Count == 0) return;
        var outputShape = OutputShapes[0];
        OutputShapes.RemoveAt(0);
        outputShape.CurrentDirection = _outputDirection;
        outputShape.Visible = true;
        switch (_outputDirection)
        {
            case ShapeViz.Direction.Up:
                break;
            case ShapeViz.Direction.Down:
                outputShape.CurrentDirection = ShapeViz.Direction.Down;
                var pos = GlobalPosition;
                var rect = outputShape.Bricks.GetUsedRect();
                var size = rect.Size * ShapeViz.TileSize;
                pos.X -= Mathf.Abs(size.X) / 2f;
                pos.Y += Mathf.Abs(size.Y);
                outputShape.GlobalPosition = pos;
                break;
            case ShapeViz.Direction.Left:
            case ShapeViz.Direction.Right:
            case ShapeViz.Direction.None:
                break;
            default:
#pragma warning disable CA2208
                throw new ArgumentOutOfRangeException();
#pragma warning restore CA2208
        }
    }
}