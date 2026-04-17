module Sim.Tests.TestShapes

module T2x2 =
  let input =
    [
      [1; 2]
      [3; 4]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let rotateLeft =
    [
      [2; 4]
      [1; 3]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let rotateRight =
    [
      [3; 1]
      [4; 2]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let flipVerticalAxis =
    [
      [2; 1]
      [4; 3]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let flipHorizontalAxis =
    [
      [3; 4]
      [1; 2]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let verticalCut =
    [
      [[1]; [3]];
      [[2]; [4]]
    ]
    |> List.map (List.map (List.map (Some >> Option.map (fun x -> x.ToString ()))))

  let horizontalCut =
    [
      [[1; 2]];
      [[3; 4]]
    ]
    |> List.map (List.map (List.map (Some >> Option.map (fun x -> x.ToString ()))))

module T2x3 = 
  let input =
    [
      [1; 2]
      [3; 4]
      [5; 6]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let rotateLeft =
    [
      [2; 4; 6]
      [1; 3; 5]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let rotateRight =
    [
      [5; 3; 1]
      [6; 4; 2]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let flipVerticalAxis =
    [
      [2; 1]
      [4; 3]
      [6; 5]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let flipHorizontalAxis =
    [
      [5; 6]
      [3; 4]
      [1; 2]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let verticalCut =
    [
      [[1]; [3]; [5]];
      [[2]; [4]; [6]]
    ]
    |> List.map (List.map (List.map (Some >> Option.map (fun x -> x.ToString ()))))

  let horizontalCut =
    [
      [[1; 2]];
      [[3; 4];
      [5; 6]]
    ]
    |> List.map (List.map (List.map (Some >> Option.map (fun x -> x.ToString ()))))

module T3x3 =
  let input =
    [
      [1; 2; 3]
      [4; 5; 6]
      [7; 8; 9]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let rotateLeft =
    [
      [3; 6; 9]
      [2; 5; 8]
      [1; 4; 7]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let rotateRight =
    [
      [7; 4; 1]
      [8; 5; 2]
      [9; 6; 3]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let flipVerticalAxis =
    [
      [3; 2; 1]
      [6; 5; 4]
      [9; 8; 7]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let flipHorizontalAxis =
    [
      [7; 8; 9]
      [4; 5; 6]
      [1; 2; 3]
    ]
    |> List.map (List.map (Some >> Option.map (fun x -> x.ToString ())))

  let verticalCut =
    [
      [[1]; [4]; [7]];
      [[2; 3]; [5; 6]; [8; 9]]
    ]
    |> List.map (List.map (List.map (Some >> Option.map (fun x -> x.ToString ()))))

  let horizontalCut =
    [
      [[1; 2; 3]];
      [[4; 5; 6];
      [7; 8; 9]]
    ]
    |> List.map (List.map (List.map (Some >> Option.map (fun x -> x.ToString ()))))

module TL =
  let input: string option list list = [
    [None; Some "A"]
    [None; Some "B"]
    [Some "C"; Some "D"]
  ]

  let rotateLeft = [
    [Some "A"; Some "B"; Some "D"]
    [None; None; Some "C"]
  ]

  let rotateRight = [
    [Some "C"; None; None]
    [Some "D"; Some "B"; Some "A"]
  ]

  let flipVerticalAxis = [
    [Some "A"; None]
    [Some "B"; None]
    [Some "D"; Some "C"]
  ]

  let flipHorizontalAxis: string option list list = [
    [Some "C"; Some "D"]
    [None; Some "B"]
    [None; Some "A"]
  ]

module TLine =
  let input: string option list list = [
    [Some "A"]
    [Some "B"]
    [Some "C"]
    [Some "D"]
  ]

  let rotateLeft = [
    [None; None; None; None]
  ]

  let rotateRight = [
    [Some "D"; Some "C"; Some "B"; Some "A"]
  ]

  let flipVerticalAxis =
    input

  let flipHorizontalAxis: string option list list = [
    [Some "D"]
    [Some "C"]
    [Some "B"]
    [Some "A"]
  ]

module TTri =
  let input: string option list list = [
    [None; Some "A"; None];
    [None; Some "B"; None];
    [Some "C"; Some "D"; Some "E"]
  ]
