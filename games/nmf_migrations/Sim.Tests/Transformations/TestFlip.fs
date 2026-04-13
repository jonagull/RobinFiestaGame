namespace Sim.Tests.Transformations.TestFlip

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Sim.Shape
open Sim.Tests

[<TestClass>]
type TestFlips () =

  [<TestMethod>]
  member _.VerticalFlip2x2 () =
    let actual = Transforms.flipVerticalAxis TestShapes.T2x2.input
    let expected = TestShapes.T2x2.flipVerticalAxis
    Assert.AreEqual(expected, actual)
  
  [<TestMethod>]
  member _.HorizontalFlip2x2 () =
    let actual = Transforms.flipHorizontalAxis TestShapes.T2x2.input
    let expected = TestShapes.T2x2.flipHorizontalAxis
    Assert.AreEqual(expected, actual)

  [<TestMethod>]
  member _.VerticalFlip2x3 () =
    let actual = Transforms.flipVerticalAxis TestShapes.T2x3.input
    let expected = TestShapes.T2x3.flipVerticalAxis
    Assert.AreEqual(expected, actual)
  
  [<TestMethod>]
  member _.HorizontalFlip2x3 () =
    let actual = Transforms.flipHorizontalAxis TestShapes.T2x3.input
    let expected = TestShapes.T2x3.flipHorizontalAxis
    Assert.AreEqual(expected, actual)

  [<TestMethod>]
  member _.VerticalFlip3x3 () =
    let actual = Transforms.flipVerticalAxis TestShapes.T3x3.input
    let expected = TestShapes.T3x3.flipVerticalAxis
    Assert.AreEqual(expected, actual)
  
  [<TestMethod>]
  member _.HorizontalFlip3x3 () =
    let actual = Transforms.flipHorizontalAxis TestShapes.T3x3.input
    let expected = TestShapes.T3x3.flipHorizontalAxis
    Assert.AreEqual(expected, actual)
  
  [<TestMethod>]
  member _.FlippingTwiceIsHomomorphic () =
    let actual = Transforms.flipHorizontalAxis TestShapes.T2x2.flipHorizontalAxis
    let expected = TestShapes.T2x2.input
    Assert.AreEqual(expected, actual)
    let actual = Transforms.flipVerticalAxis TestShapes.T2x2.flipVerticalAxis
    let expected = TestShapes.T2x2.input
    Assert.AreEqual(expected, actual)
  
