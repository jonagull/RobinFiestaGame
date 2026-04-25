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

    public override void _Ready()
    {
        GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D")?.Play();
        base._Ready();
    }

    protected override void TransformShape(ShapeViz shapeViz)
    {
        var shape = shapeViz.Shape;
        var firstIter = true;
        var ys = _cutter.CutShape(shape);
        GD.Print(ys.Length);
        foreach (var newShape in ys)
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
