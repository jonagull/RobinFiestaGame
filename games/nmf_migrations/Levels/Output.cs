using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.FSharp.Core;
using Sim;

public partial class Output : Node2D
{
    [Export]
    private string[] _rawShapeTypes;

    private List<Shapes.ShapeType> _shapeTypes;

    private readonly PackedScene _shapeViz =
        GD.Load<PackedScene>("res://games/nmf_migrations/Viz/shape.tscn");

    private readonly Dictionary<Shapes.ShapeType, PackedScene> _shapes = new()
    {
        {
            Shapes.ShapeType.TwoByTwo,
            GD.Load<PackedScene>($"res://games/nmf_migrations/Shapes/{Shapes.ShapeType.TwoByTwo}.tscn")
        },
        {
            Shapes.ShapeType.L,
            GD.Load<PackedScene>($"res://games/nmf_migrations/Shapes/{Shapes.ShapeType.L}.tscn")
        },
        {
            Shapes.ShapeType.Line,
            GD.Load<PackedScene>($"res://games/nmf_migrations/Shapes/{Shapes.ShapeType.Line}.tscn")
        },
        {
            Shapes.ShapeType.Tri,
            GD.Load<PackedScene>($"res://games/nmf_migrations/Shapes/{Shapes.ShapeType.Tri}.tscn")
        },
    };

    [Export]
    private bool _started;
    
    [Export]
    private double _shapeDeltaTime;

    [Export]
    private Vector2 _spawnPoint;
    
    private double _time;

    private int _shapePointer;

    public override void _EnterTree()
    {
        _shapeTypes = _rawShapeTypes
            .Select(Shapes.StringToShapeType)
            .Where(FSharpOption<Shapes.ShapeType>.get_IsSome)
            .Select(o => o.Value)
            .ToList();
    }

    public override void _Ready()
    {
        if (_shapes == null || _shapeTypes.Count == 0)
        {
            throw new Exception("_shapes is null or empty");
        }
    }

    private void SpawnShape()
    {
        if (_shapePointer >= _shapeTypes.Count)
        {
            _started = false;
            return;
        }
        
        var shapeType = _shapeTypes[_shapePointer];
        var tileMapShape = _shapes[shapeType];
        var tileMapNode = tileMapShape.Instantiate<TileMapLayer>();
        tileMapNode.Name = "TileMapLayer";
        var shapeVizNode = _shapeViz.Instantiate<ShapeViz>();
        shapeVizNode.IsCuttable = true;
        shapeVizNode.AddChild(tileMapNode);
        AddChild(shapeVizNode);
        shapeVizNode.GlobalPosition = GlobalPosition + _spawnPoint;
        _shapePointer++;
    }

    public override void _Process(double delta)
    {
        if (!_started)
        {
            return;
        }
        _time += delta;
        if (!(_time >= _shapeDeltaTime)) return;
        _time = 0;
        SpawnShape();
    }
}
