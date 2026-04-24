using Godot;
using Sim;

public partial class Cut : MachineViz
{
    [Export] private string _cutDirection = "vertical"; 
    
    private Cutter.Cutter _cutter;

    public override void _EnterTree()
    {
        var cut =
            Shapes.getOrThrow(Shapes.StringToCutType(_cutDirection), $"Invalid rotation: {_cutDirection}");
        _cutter = new Cutter.Cutter(cut);
    }

    public override void _Process(double delta)
    {
        GD.Print(OutputShapes.Count);
        base._Process(delta);
    }

    protected override void TransformShape(ShapeViz shapeViz)
    {
        var shape = shapeViz.Shape;
        var firstIter = true;
        foreach (var newShape in _cutter.CutShape(shape))
        {
            if (firstIter)
            {
                firstIter = false;
                shapeViz.SetShape(newShape);
                AddOutput(shapeViz);
                continue;
            }
            var dup = shapeViz.Duplicate();
            var dupShapeViz = dup.GetNode<ShapeViz>(".");
            dupShapeViz.SetFirstShape(newShape);
            AddOutput(dupShapeViz);
        }
    }
}
