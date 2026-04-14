module Sim.Tests.TestShapes

module T2x2 =
  let input = [
    [1; 2]
    [3; 4]
  ]

  let rotateLeft = [
    [2; 4]
    [1; 3]
  ]

  let rotateRight = [
    [3; 1]
    [4; 2]
  ]

  let flipVerticalAxis = [
    [2; 1]
    [4; 3]
  ]

  let flipHorizontalAxis = [
    [3; 4]
    [1; 2]
  ]

module T2x3 = 
  let input = [
    [1; 2]
    [3; 4]
    [5; 6]
  ]

  let rotateLeft = [
    [2; 4; 6]
    [1; 3; 5]
  ]

  let rotateRight = [
    [5; 3; 1]
    [6; 4; 2]
  ]

  let flipVerticalAxis = [
    [2; 1]
    [4; 3]
    [6; 5]
  ]

  let flipHorizontalAxis = [
    [5; 6]
    [3; 4]
    [1; 2]
  ]

module T3x3 =
  let input = [
    [1; 2; 3]
    [4; 5; 6]
    [7; 8; 9]
  ]

  let rotateLeft = [
    [3; 6; 9]
    [2; 5; 8]
    [1; 4; 7]
  ]

  let rotateRight = [
    [7; 4; 1]
    [8; 5; 2]
    [9; 6; 3]
  ]

  let flipVerticalAxis = [
    [3; 2; 1]
    [6; 5; 4]
    [9; 8; 7]
  ]

  let flipHorizontalAxis = [
    [7; 8; 9]
    [4; 5; 6]
    [1; 2; 3]
  ]

module TL =
  let input: string option list list = [
    [None; Some "A"]
    [None; Some "B"]
    [Some "C"; Some "D"]
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
    [None; Some "A"]
    [None; Some "B"]
    [None; Some "C"]
    [None; Some "D"]
  ]

  let flipVerticalAxis = [
    [Some "A"; None]
    [Some "B"; None]
    [Some "C"; None]
    [Some "D"; None]
  ]

  let flipHorizontalAxis: string option list list = [
    [None; Some "D"]
    [None; Some "C"]
    [None; Some "B"]
    [None; Some "A"]
  ]

module TTri =
  let input: string option list list = [
    [None; Some "A"; None];
    [None; Some "B"; None];
    [Some "C"; Some "D"; Some "E"]
  ]
