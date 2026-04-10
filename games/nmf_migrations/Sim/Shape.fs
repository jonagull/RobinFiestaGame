module Sim.Shapes 

open Sim.Shape.Transforms
open Sim.Shape.Cuts

type Rotation
  = Left
  | Right

type Cut =
  | Horizontal
  | Vertical

type Transformation
  = RotateLeft
  | RotateRight
  | FlipHorizontal
  | FlipVertical

type ShapeType =
  | TwoByTwo = 0
  | L = 1
  | Line = 2
  | Tri = 3

let StringToShapeType (s: string) : ShapeType Option =
  match s.ToLower() with
  | "2x2" | "twobytwo" -> Some ShapeType.TwoByTwo
  | "l" -> Some ShapeType.L
  | "line" -> Some ShapeType.Line
  | "tri" -> Some ShapeType.Tri
  | _ -> None

type Brick =
  struct
    val color: string
    new(color: string) = { color = color }
  end

type Shape(bricks: Brick Option list list) =

  let mutable bricks = bricks;

  let setBricks newBricks =
    bricks <- newBricks

  let printXs (xs : Brick Option list list): unit =
    Godot.GD.Print (
      xs
      |> List.map (fun xs ->
        xs
        |> List.map (fun x ->
          match x with
          | Some b -> b.color
          | None -> "None"
        )
        |> List.fold (fun acc c -> $"{acc}{c},") ""
        |> fun x -> x.ToCharArray()
        |> Array.rev
        |> fun xs -> Array.sub xs 1 (xs.Length - 1)
        |> Array.rev
        |> Array.fold (fun acc c -> $"{acc}{c}") ""
      )
      |> List.fold (fun acc c -> $"{acc}{c}\n") ""
    )
    |> ignore

    
  static member createShape (
    xs : System.Nullable<Brick>
      System.Collections.Generic.List
      System.Collections.Generic.List
  ) : Shape =
    xs
    |> List.ofSeq
    |> List.map (
      fun xs ->
        xs
        |> List.ofSeq
        |> List.map Option.ofNullable
    )
    |> Shape
    
  member _.GetBricks =
    bricks
    |> List.map (
      fun xs ->
        xs
        |> List.map Option.toNullable
        |> List.toArray
    )
    |> List.toArray

  member _.Bricks = bricks

  member this.Rotate (trans: Transformation) =
    let newBricks =
      match trans with
      | RotateRight ->
        rotateRight this.Bricks
      | RotateLeft ->
        rotateLeft this.Bricks
      | FlipHorizontal ->
        flipHorizontalAxis this.Bricks
      | FlipVertical ->
        flipVerticalAxis this.Bricks
    setBricks newBricks
  
  member this.Cut (cut : Cut) =
    let f, s =
      match cut with
      | Horizontal -> cutHorizontal this.Bricks
      | Vertical -> cutVertical this.Bricks
    setBricks f
    Shape s
  
