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
  So only changed when we see an empty row or column
*)
let separate (xs : 'a option list list) : 'a option list list list =
  (* 
  *)
  let rowFilter (rs : 'a option list list) : 'a option list list list =
    rs
    |> List.fold (
      fun (acc, xs) cur ->
        if List.fold (fun b x -> b && Option.isNone x) true cur then
          xs :: acc, []
        else
          acc, cur :: xs
    ) ([], [])
    |> fun (xs, ys) -> ys :: xs
  let removeColumn (idx : int) (cs : 'a option list list) : 'a option list list =
    cs
  let columnFilter (cs : 'a option list list) : 'a option list list list =
    cs
    |> List.fold (
      fun (acc, xs) cur ->
        let count = List.map (fun x -> if Option.isNone x then 1 else 0) cur
        acc,
        List.zip xs count
        |> List.map (fun (x, y) -> x + y)
    ) ([], [])
    |> fun (xs: 'a option list list, ys) ->
      ys
      |> List.fold (fun (acc, idx) cur ->
        if xs[idx].Length = cur then
          removeColumn idx acc, idx
        else
          acc, idx + 1
      ) ([], 0)
      |> fun (xs, _) -> [xs]
  [xs]

let cutHorizontal (xs : 'a option list list) : 'a option list list list =
  xs
  |> List.take (xs.Length / 2)
  |> fun ys ->
    xs
    |> List.skip (xs.Length / 2)
    |> fun zs -> [ys; zs]

let cutVertical (xs : 'a option list list) : 'a option list list list =
  let halfWidth = xs.Head.Length / 2
  [xs
  |> List.map (List.take halfWidth);
  xs
  |> List.map (List.skip halfWidth)]
