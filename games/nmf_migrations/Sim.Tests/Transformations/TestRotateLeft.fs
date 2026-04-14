namespace Sim.Tests.Transformations.TestRotateLeft

open Microsoft.VisualStudio.TestTools.UnitTesting
open Sim.Shape
open Sim.Tests

[<TestClass>]
type RotateLeft () =

  [<TestMethod>]
  member _.RotateLeftOn2x2 () =
    let actual = Transforms.rotateLeft TestShapes.T2x2.input
    Assert.AreEqual(TestShapes.T2x2.rotateLeft, actual)

  [<TestMethod>]
  member _.RotateLeftOn2x3 () =
    let actual = Transforms.rotateLeft TestShapes.T2x3.input
    Assert.AreEqual(TestShapes.T2x3.rotateLeft, actual)

  [<TestMethod>]
  member _.RotateLeftOn3x3 () =
    let input = [
      [1; 2; 3]
      [4; 5; 6]
      [7; 8; 9]
    ]
    let expected = [
      [3; 6; 9]
      [2; 5; 8]
      [1; 4; 7]
    ]
    let actual = Transforms.rotateLeft input
    Assert.AreEqual(expected, actual)
