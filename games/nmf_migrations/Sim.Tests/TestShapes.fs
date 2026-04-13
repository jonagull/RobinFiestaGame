module Sim.Tests.TestShapes

module T2x2 =
  let input = [
    [1; 2]
    [3; 4]
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

module Shapes =
  let L: string option list list = [
    [None; Some "A"];
    [None; Some "B"];
    [Some "C"; Some "D"]
  ]


  let Line: string option list list = [
    [None; Some "A"];
    [None; Some "B"];
    [None; Some "C"]
    [None; Some "D"]
  ]

  let Tri: string option list list = [
    [None; Some "A"; None];
    [None; Some "B"; None];
    [Some "C"; Some "D"; Some "E"]
  ]