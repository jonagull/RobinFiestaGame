module Sim.Shapes 

open Sim.Shape.Transforms
open Sim.Shape.Cuts

type Rotation
  = Left
  | Right

type Cut =
  | Horizontal = 0
  | Vertical = 1

#nowarn "104"
type Transformation
  = RotateLeft = 0
  | RotateRight = 1
  | FlipHorizontal = 2
  | FlipVertical = 3

type ShapeType =
  | TwoByTwo = 0
  | L = 1
  | Line = 2
  | Tri = 3

let StringToTransformationType (s : string) : Transformation option =
  match s.ToLower() with
  | "l" | "rl" | "left" -> Some Transformation.RotateLeft
  | "r" | "rr" | "right" -> Some Transformation.RotateRight
  | "h" | "fh" | "fliph" -> Some Transformation.FlipHorizontal
  | "v" | "fv" | "flipv" -> Some Transformation.FlipVertical
  | _ -> None


let StringToCutType (s : string) : Cut option =
  match s.ToLower() with
  | "h" | "horizontal" -> Some Cut.Horizontal
  | "v" | "vertical" -> Some Cut.Vertical
  | _ -> None

let getOrThrow (o : 'a option) (msg : string) : 'a =
  match o with
  | Some v -> v
  | None -> raise (new System.Exception (msg))

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

  member this.Rotate (trans: Transformation) : Shape =
    let newBricks =
      match trans with
      | Transformation.RotateRight ->
        rotateRight this.Bricks
      | Transformation.RotateLeft ->
        rotateLeft this.Bricks
      | Transformation.FlipHorizontal ->
        flipHorizontalAxis this.Bricks
      | Transformation.FlipVertical ->
        flipVerticalAxis this.Bricks
    Shape newBricks
  
  member this.Cut (cut : Cut) =
    match cut with
    | Cut.Horizontal -> cutHorizontal this.Bricks
    | Cut.Vertical -> cutVertical this.Bricks
    |> List.map Shape
  
