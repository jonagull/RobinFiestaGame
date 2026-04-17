module Sim.Shape.Cuts

(*
cases:
  [
    [None, Some];
    [None, Some];
  ]
  ->
  [
    [Some];
    [Some];
  ]
  [
    [None, Some];
    [Some, None];
  ]
  ->
  [
    [None, Some];
    [Some, None];
  ]
*)
let separate (xs : 'a option list list) : 'a option list list list =
  [xs]

let cutHorizontal (xs : 'a option list list) : 'a option list list list =
  xs
  |> List.take (xs.Length / 2)
  |> separate
  |> fun ys ->
    xs
    |> List.skip (xs.Length / 2)
    |> separate
    |> fun zs -> List.concat [ys; zs]

let cutVertical (xs : 'a option list list) : 'a option list list list =
  let halfWidth = xs.Head.Length / 2
  [xs
  |> List.map (List.take halfWidth);
  xs
  |> List.map (List.skip halfWidth)]
  |> List.map separate
  |> List.concat
