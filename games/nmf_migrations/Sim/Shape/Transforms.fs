module Sim.Shape.Transforms

let headRows (xs : 'a list list) : 'a list * 'a list list =
  let foldHelper ((hs, ys) : 'a list * 'a list list) (zs : 'a list) =
    match zs with
    | z :: zs -> z :: hs, zs :: ys
    | _ -> hs, ys
  
  xs
  |> List.fold foldHelper ([], []) 
  |> fun (f, s) -> f, List.rev s

let rec rotateRight (xs : 'a list list) : 'a list list =
  match headRows xs with
  | [], [] -> []
  | hs, ys -> hs :: rotateRight ys

let flipHorizontalAxis : 'a list list -> 'a list list =
  List.rev

let flipVerticalAxis (xs : 'a list list) : 'a list list =
  List.map List.rev xs

let rotateLeft (xs : 'a list list) : 'a list list =
  xs
  |> List.map List.rev
  |> rotateRight
  |> List.map List.rev
