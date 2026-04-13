using Godot;

public partial class MachinePlacer : Node
{
    private TileMapLayer _machineTml;
    private TileMapLayer _ioTml;
    private TileMapLayer _backgroundTml;
    
    public override void _Ready()
    {
        _machineTml = GetNode<TileMapLayer>("../Machine");
        _ioTml = GetNode<TileMapLayer>("../IO");
        _backgroundTml = GetNode<TileMapLayer>("../Background");
    }

    private void PlaceMachine(Vector2 localMousePosition)
    {
        var cellPosition = _machineTml.LocalToMap(localMousePosition);
    }

    private bool IsValidPlacement(Vector2I cellPosition)
    {
        var surroundingMachineCells = _machineTml.GetSurroundingCells(cellPosition);
        var noMachineAround = surroundingMachineCells.Count == 0;
        var surroundingIoCells = _ioTml.GetSurroundingCells(cellPosition);
        var noIoCells = surroundingIoCells.Count == 0;
        var surroundingBackgroundCells = _backgroundTml.GetSurroundingCells(cellPosition);
        var noBackgroundCells = surroundingBackgroundCells.Count == 0;
        return noMachineAround && noIoCells && noBackgroundCells;
    }
}
