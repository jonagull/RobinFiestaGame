module Sim.Shapes 

type Rotation
    = Left
    | Right

type Brick(color: string) =
    member _.color = color

type Shape(bricks: (Brick Option) list list) =
    let mutable bricks = bricks;

    let setBricks newBricks = bricks <- newBricks

    static member createShape (xs : Brick Option Godot.Collections.Array Godot.Collections.Array) : Shape =
        [
            for i in 0 .. xs.Count -> [
                for j in 0 .. xs[0].Count -> xs[i][j]
            ]
        ]
        |> Shape

    member _.Bricks = bricks

    member this.Rotate (rot: Rotation) =
        match rot with
        | Right -> List.transpose this.Bricks |> setBricks
        | Left -> this.Bricks |> List.transpose |> List.transpose |> List.transpose |> setBricks 