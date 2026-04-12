using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Sim;

public partial class ShapeViz : StaticBody2D
{
    private const int TileSize = 32;

    public TileMapLayer Bricks  { get; set; }

    private CollisionShape2D _collisionShape2D;

    private float _speed = 1;
    
    public Shapes.Shape Shape { get; private set; }
    
    private static List<List<Shapes.Brick?>> FromTileMap(TileMapLayer tileMap)
    {
        var xs = new List<List<Shapes.Brick?>>();

        var tiles = tileMap.GetUsedCells();
        var minX = tiles.MinBy(t => t.X).X;
        var minY = tiles.MinBy(t => t.Y).Y;
        var maxX = tiles.MaxBy(t => t.X).X;
        var maxY = tiles.MaxBy(t => t.Y).Y;

        var xScew = minX < 0 ? Math.Abs(minX) : -minX;
        var yScew = minY < 0 ? Math.Abs(minY) : -minY;

        for (var x = minX - xScew; x <= maxX - xScew; x++)
        {
            List<Shapes.Brick?> row = [];
            for (var y = minX - yScew; y <= maxY - yScew; y++)
            {
                var coords = tileMap.GetCellAtlasCoords(new Vector2I(x, y));
                row.Add(coords is { X: -1, Y: -1 } ? null : new Shapes.Brick(ColorFromTile(coords)));
            }

            xs.Add(row);
        }
        
        GD.Print(xs.Select(x => x.Select(y => y.HasValue ? y.Value.color : "null").Aggregate("", (acc, c) => acc + $"{c},")).Aggregate("", (acc, c) => acc + $"{c}\n"));

        return xs;
    }

    public void SetShape(Shapes.Shape shape)
    {
        Shape = shape;
        OnShapeChange();
    }

    public override void _Ready()
    {
        Bricks = GetNodeOrNull<TileMapLayer>("TileMapLayer")
                  ?? GetNodeOrNull<TileMapLayer>("FallBack")
                  ?? throw new Exception("No TileMapLayer found");
        _collisionShape2D = GetNodeOrNull<CollisionShape2D>("Area2D/CollisionShape2D")
                            ?? throw new Exception("No collision shape found");
        Shape = Shapes.Shape.createShape(FromTileMap(Bricks));
        OnShapeChange();
        CallDeferred("PostOnChangeShape");
    }

    public void OnShapeChange()
    {
        Bricks.Clear();
        const int sourceId = 0;
        var bricks = Shape.GetBricks;
        for (var i = 0; i < bricks.Length; i++)
        for (var j = 0; j < bricks[0].Length; j++)
        {
            var brick = bricks[i][j];
            if (brick != null)
                Bricks.SetCell(new Vector2I(i, j), sourceId, TileFromColor(brick.Value.color));
            else
                Bricks.SetCell(new Vector2I(i, j));
        }
    }

    public void PostOnChangeShape()
    {
        var targetSize = Bricks.GetUsedRect().Size * TileSize;
        _collisionShape2D.Shape = new RectangleShape2D { Size = targetSize };
        _collisionShape2D.Position = targetSize / 2;
    }

    public override void _Process(double delta)
    {
        MoveAndCollide(new Vector2(0, _speed));
    }

    public static Vector2I TileFromColor(string color)
    {
        return color switch
        {
            "black" => new Vector2I(0, 0),
            "white" => new Vector2I(1, 0),
            "red" => new Vector2I(2, 0),
            "green" => new Vector2I(0, 1),
            "brown" => new Vector2I(1, 1),
            "yellow" => new Vector2I(2, 1),
            "blue" => new Vector2I(0, 2),
            "pink" => new Vector2I(1, 2),
            "orange" => new Vector2I(2, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(color), $"no match for {color}")
        };
    }

    public static string ColorFromTile(Vector2I tile)
    {
        return (tile.X, tile.Y) switch
        {
            (0, 0) => "black",
            (1, 0) => "white",
            (2, 0) => "red",
            (0, 1) => "green",
            (1, 1) => "brown",
            (2, 1) => "yellow",
            (0, 2) => "blue",
            (1, 2) => "pink",
            (2, 2) => "orange",
            _ => throw new ArgumentOutOfRangeException(nameof(tile), tile, null)
        };
    }
}
