module Sim.Tests.TestShapes

module T2x2 =
  let input = [
    [1; 2]
    [3; 4]
  ]

  let flipVertical = [
    [2; 1]
    [4; 3]
  ]

  let flipHorizontal = [
    [3; 4]
    [1; 2]
  ]
(*
module foo = 
  let _2x3 = [
    [1; 2]
    [3; 4]
    [5; 6]
  ]

  let _3x3 = [
    [1; 2; 3]
    [4; 5; 6]
    [7; 8; 9]
  ]

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
*)