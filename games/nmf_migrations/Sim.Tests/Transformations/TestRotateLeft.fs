namespace Sim.Tests.Transformations.TestRotateLeft

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Sim.Shape

[<TestClass>]
type RotateLeft () =

  [<TestMethod>]
  member _.RotateLeftOn2x2 () =
    let input = [
      [1; 2]
      [3; 4]
    ]
    let expected = [
      [2; 4]
      [1; 3]
    ]
    let actual = Transforms.rotateLeft input
    Assert.AreEqual(expected, actual)

  [<TestMethod>]
  member _.RotateLeftOn2x3 () =
    let input = [
      [1; 2]
      [3; 4]
      [5; 6]
    ]
    let expected = [
      [2; 4; 6]
      [1; 3; 5]
    ]
    let actual = Transforms.rotateLeft input
    Assert.AreEqual(expected, actual)

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
