module Sim.Shape.Cuts

let cutHorizontal (xs : 'a list list) : 'a list list * 'a list list =
  List.take (xs.Length / 2) xs,
  List.skip (xs.Length / 2) xs

let cutVertical (xs : 'a list list) : 'a list list * 'a list list =
  let halfWidth = xs.Head.Length / 2
  xs
  |> List.map (List.take halfWidth),
  xs
  |> List.map (List.skip halfWidth)