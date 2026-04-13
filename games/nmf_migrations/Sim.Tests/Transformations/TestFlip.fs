namespace Sim.Tests.Transformations.TestFlip

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Sim.Shape
open Sim.Tests

[<TestClass>]
type TestFlips () =

  [<TestMethod>]
  member _.VerticalFlip2x2 () =
    let actual = Transforms.flipVertical TestShapes.T2x2.input
    let expected = TestShapes.T2x2.flipVertical
    Assert.AreEqual(expected, actual)
  

  [<TestMethod>]
  member _.HorizontalFlip2x2 () =
    let actual = Transforms.flipHorizontal TestShapes.T2x2.input
    let expected = TestShapes.T2x2.flipHorizontal
    Assert.AreEqual(expected, actual)