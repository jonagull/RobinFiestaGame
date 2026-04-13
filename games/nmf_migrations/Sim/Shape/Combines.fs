module Sim.Shape.Combines

let combineHorizontal (xs : 'a list list) (ys : 'a list list) : 'a list list =
  List.concat [xs; ys]

let rec combineVertical (xs : 'a list list) (ys : 'a list list) : 'a list list =
  match xs, ys with
  | [], [] -> []
  | x :: xs, y :: ys -> List.concat [x; y] :: combineVertical xs ys
  | _, _ -> raise (new System.Exception "Missmatch length on xs and ys")