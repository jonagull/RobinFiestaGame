namespace Sim.Tests.Transformations.TestVerticalCuts

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Sim.Shape

[<TestClass>]
type TestVerticalCuts () =

  let _2x2 = [
    [1; 2]
    [3; 4]
  ]
  let _2x2Result = (
    [[1]; [3]],
    [[2]; [4]]
  )

  let _2x3 = [
    [1; 2]
    [3; 4]
    [5; 6]
  ]
  let _2x3Result = (
    [[1]; [3]; [5]],
    [[2]; [4]; [6]]
  )
  
  let _3x3 = [
    [1; 2; 3]
    [4; 5; 6]
    [7; 8; 9]
  ]
  let _3x3Result = (
    [[1]; [4]; [7]],
    [[2; 3]; [5; 6]; [8; 9]]
  )

  [<TestMethod>]
  member _.CutOn2x2 () =
    let actual = Cuts.cutVertical _2x2
    Assert.AreEqual(_2x2Result, actual)

  [<TestMethod>]
  member _.CutOn2x3 () =
    let actual = Cuts.cutVertical _2x3
    Assert.AreEqual(_2x3Result, actual)

  [<TestMethod>]
  member _.CutOn3x3 () =
    let actual = Cuts.cutVertical _3x3
    Assert.AreEqual(_3x3Result, actual)
  

  [<TestMethod>]
  member _.CutsAndCombinesAreHomomorphic () =
    [_2x2; _2x3; _3x3]
    |> List.map (fun input ->
      Assert.AreEqual(
        input,
        input
        |> Cuts.cutVertical
        |> fun (xs, ys) -> Combines.combineVertical xs ys
      )
    )
    |> ignore
