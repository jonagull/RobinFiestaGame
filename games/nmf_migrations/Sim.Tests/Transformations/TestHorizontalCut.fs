namespace Sim.Tests.Transformations.TestHorizontalCuts

open Microsoft.VisualStudio.TestTools.UnitTesting
open Sim.Shape
open Sim.Tests

[<TestClass>]
type TestHorizontalCuts () =

  [<TestMethod>]
  member _.CutOn2x2 () =
    let actual = Cuts.cutHorizontal TestShapes.T2x2.input
    Assert.AreEqual(TestShapes.T2x2.horizontalCut, actual)

  [<TestMethod>]
  member _.CutOn2x3 () =
    let actual = Cuts.cutHorizontal TestShapes.T2x3.input
    Assert.AreEqual(TestShapes.T2x3.horizontalCut, actual)

  [<TestMethod>]
  member _.CutOn3x3 () =
    let actual = Cuts.cutHorizontal TestShapes.T3x3.input
    Assert.AreEqual(TestShapes.T3x3.horizontalCut, actual)

  [<TestMethod>]
  member _.CutOnU () =
    let actual = Cuts.cutHorizontal TestShapes.TU.input
    Assert.AreEqual(TestShapes.TU.horizontalCut, actual)
  
  [<TestMethod>]
  member _.CutsAndCombinesAreHomomorphic () =
    [ TestShapes.T2x2.input
    ; TestShapes.T2x3.input
    ; TestShapes.T3x3.input
    ]
    |> List.map (fun input ->
      Assert.AreEqual(
        input,
        input
        |> Cuts.cutHorizontal
        |> List.fold Combines.combineHorizontal []
        |> fun x -> x
      )
    )
    |> ignore
