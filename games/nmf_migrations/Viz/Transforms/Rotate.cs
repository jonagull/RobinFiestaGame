using System;
using Godot;
using Sim;

namespace RobinFiesta.games.nmf_migrations.Viz.Transforms;

public partial class Rotate : MachineViz
{
    [Export] private string _rotation = "left"; 
    
    private Machine.Machine _machine;

    public override void _EnterTree()
    {
        var trans =
            Shapes.getOrThrow(Shapes.StringToTransformationType(_rotation), $"Invalid rotation: {_rotation}");
        _machine = new Transformator.Transformator(trans);
    }

    protected override void TransformShape(ShapeViz shapeViz)
    {
        var shape = shapeViz.Shape;
        var newShape = _machine.WorkShape(shape);
        shapeViz.SetShape(newShape);
        AddOutput(shapeViz);
    }
}