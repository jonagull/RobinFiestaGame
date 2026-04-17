namespace Sim.Tests.Transformations.TestVerticalCuts

open Microsoft.VisualStudio.TestTools.UnitTesting
open Sim.Shape
open Sim.Tests

[<TestClass>]
type TestVerticalCuts () =
  
  [<TestMethod>]
  member _.CutOn2x2 () =
    let actual = Cuts.cutVertical TestShapes.T2x2.input
    Assert.AreEqual(TestShapes.T2x2.verticalCut, actual)

  [<TestMethod>]
  member _.CutOn2x3 () =
    let actual = Cuts.cutVertical TestShapes.T2x3.input
    Assert.AreEqual(TestShapes.T2x3.verticalCut, actual)

  [<TestMethod>]
  member _.CutOn3x3 () =
    let actual = Cuts.cutVertical TestShapes.T3x3.input
    Assert.AreEqual(TestShapes.T3x3.verticalCut, actual)
  

  [<TestMethod>]
  member _.CutsAndCombinesAreHomomorphic () =
    [TestShapes.T2x2.input; TestShapes.T2x3.input; TestShapes.T3x3.input]
    |> List.map (fun input ->
      Assert.AreEqual(
        input,
        input
        |> Cuts.cutVertical
        |> List.fold Combines.combineVertical []
      )
    )
    |> ignore
