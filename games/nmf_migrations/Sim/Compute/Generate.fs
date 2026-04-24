module Sim.Compute.Generate

open Sim.Shapes
open System

type Pixel =
  Pixel of x : int * y : int

let lookup
  (xs : Pixel option list list)
  (i: int)
  (j : int)
  : (int * int) option =
  xs
  |> List.skip i
  |> List.head
  |> List.skip j
  |> List.head
  |> Option.map (fun (Pixel (x, y)) -> x, y)

let coordinate (xs : 'a list list) : ('a * int * int) list =
  xs
  |> List.zip [0..(xs.Length + 1)]
  |> List.map (fun (i, ys) ->
    ys
    |> List.zip [0..(ys.Length + 1)]
    |> List.map (fun (j, x) -> x, i, j)
  )
  |> List.concat
  
let put (xs : 'a list list) (i : int) (j : int) (a : 'a) : 'a list list =
  let row = xs[i]
  let row' = List.concat [List.take j row; a :: List.skip (j + 1) row]
  List.concat [List.take i xs; row' :: List.skip (i + 1) xs]

(*
  Uses Pixel data type to store transformation information, the x,y positions in
  a Pixel
  Pixel.x, Pixel.y -> Brick
  Brick row and column -> Pixel
*)
type ShapeRep (shape : Pixel option list list) =
  member private this.lookup =
    lookup this.Shape

  member _.Shape =
    shape
  
  member this.ToShape (xs : Brick option list list) : Shape =
    xs
    |> coordinate
    |> List.fold (fun acc (b, i, j) ->
      match this.lookup i j with
      | Some (x, y) -> put acc x y b
      | None -> acc 
    ) (List.map (fun ys -> List.map (fun _ -> None) ys) this.Shape)
    |> Shape