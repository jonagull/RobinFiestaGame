module Sim.Compute.Generate

open Sim.Shapes
open System

type Pixel
  = Present of id : string * x : uint * y : uint
  | Missing of id : string * x : uint * y : uint

(*
  Uses Pixel data type to store transformation information, the x,y positions in
  a Pixel
  Pixel.x, Pixel.y -> Brick
  Brick row and column -> Pixel
*)
type ShapeRep (shape : Pixel list list) =
  member private this.lookup (x: int) (y: int) : Pixel option =
    this.Shape
    |> List.skip x
    |> List.head
    |> List.skip y
    |> List.head
    |> Some


  member _.Shape =
    shape
  
  member this.ToShape (xs : (Brick * uint * uint) list) : Shape =
    xs
    |> List.map (
      fun (b, x, y) ->
        [Some b]
    )
    |> Shape