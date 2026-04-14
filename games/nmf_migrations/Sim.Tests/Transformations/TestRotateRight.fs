namespace Sim.Tests.Transformations.TestRotateRight

open Microsoft.VisualStudio.TestTools.UnitTesting
open Sim.Shape
open Sim.Tests

[<TestClass>]
type RotateRight () =

  [<TestMethod>]
  member _.RotateRightOn2x2 () =
    let actual = Transforms.rotateRight TestShapes.T2x2.input
    Assert.AreEqual(TestShapes.T2x2.rotateRight, actual)

  [<TestMethod>]
  member _.RotateRightOn2x3 () =
    let actual = Transforms.rotateRight TestShapes.T2x3.input
    Assert.AreEqual(TestShapes.T2x3.rotateRight, actual)

  [<TestMethod>]
  member _.RotateRightOn3x3 () =
    let actual = Transforms.rotateRight TestShapes.T3x3.input
    Assert.AreEqual(TestShapes.T3x3.rotateRight, actual)

  [<TestMethod>]
  member _.RotateRightOnL () =
    let actual = Transforms.rotateRight TestShapes.TL.input
    Assert.AreEqual(TestShapes.TL.rotateRight, actual)

  [<TestMethod>]
  member _.RotateRightOnLine () =
    let actual = Transforms.rotateRight TestShapes.TLine.input
    Assert.AreEqual(TestShapes.TLine.rotateRight, actual)

  [<TestMethod>]
  member _.RotateRightOnTri () =
    let actual = Transforms.rotateRight TestShapes.TTri.input
    Assert.AreEqual(TestShapes.TTri.rotateRight, actual)
    
  [<TestMethod>]
  member _.TestHeadRows () =
    let input = [
      [1; 2]
      [3; 4]
    ]
    let expected =
      [3; 1],      
      [
        [2];
        [4]
      ]
    let actual = Transforms.headRows input
    Assert.AreEqual(expected, actual)
  
  [<TestMethod>]
  member _.TestHeadRowsOnEmptyInput () =
    let input: int list list = []
    let expected = 
      [],      
      []
    let actual = Transforms.headRows input
    Assert.AreEqual(expected, actual)
