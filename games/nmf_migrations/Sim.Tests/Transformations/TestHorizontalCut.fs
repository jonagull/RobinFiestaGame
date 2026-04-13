namespace Sim.Tests.Transformations.TestHorizontalCuts

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Sim.Shape

[<TestClass>]
type TestHorizontalCuts () =

  let _2x2: int list list = [
    [1; 2]
    [3; 4]
  ]

  let _2x2Result: int list list * int list list = (
    [[1; 2]],
    [[3; 4]]
  )

  let _2x3: int list list = [
    [1; 2]
    [3; 4]
    [5; 6]
  ]
  let _2x3Result: int list list * int list list = (
    [[1; 2]],
    [[3; 4];
      [5; 6]]
  )

  let _3x3: int list list = [
    [1; 2; 3]
    [4; 5; 6]
    [7; 8; 9]
  ]
  let _3x3Result: int list list * int list list = (
    [[1; 2; 3]],
    [[4; 5; 6];
      [7; 8; 9]]
  )

  [<TestMethod>]
  member _.CutOn2x2 () =
    let actual = Cuts.cutHorizontal _2x2
    Assert.AreEqual(_2x2Result, actual)

  [<TestMethod>]
  member _.CutOn2x3 () =
    let actual = Cuts.cutHorizontal _2x3
    Assert.AreEqual(_2x3Result, actual)

  [<TestMethod>]
  member _.CutOn3x3 () =
    let actual = Cuts.cutHorizontal _3x3
    Assert.AreEqual(_3x3Result, actual)
  
  [<TestMethod>]
  member _.CutsAndCombinesAreHomomorphic () =
    [_2x2; _2x3; _3x3]
    |> List.map (fun input ->
      Assert.AreEqual(
        input,
        input
        |> Cuts.cutHorizontal
        |> fun (xs, ys) -> Combines.combineHorizontal xs ys
      )
    )
    |> ignore
