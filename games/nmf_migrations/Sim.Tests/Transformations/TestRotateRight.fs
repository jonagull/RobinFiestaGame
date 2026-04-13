namespace Sim.Tests.Transformations.TestRotateRight

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Sim.Shape

[<TestClass>]
type RotateRight () =

  [<TestMethod>]
  member this.RotateRightOn2x2 () =
    let input = [
      [1; 2]
      [3; 4]
    ]
    let expected = [
      [3; 1]
      [4; 2]
    ]
    let actual = Transforms.rotateRight input
    Assert.AreEqual(expected, actual)

  [<TestMethod>]
  member this.RotateRightOn2x3 () =
    let input = [
      [1; 2]
      [3; 4]
      [5; 6]
    ]
    let expected = [
      [5; 3; 1]
      [6; 4; 2]
    ]
    let actual = Transforms.rotateRight input
    Assert.AreEqual(expected, actual)

  [<TestMethod>]
  member this.RotateRightOn3x3 () =
    let input = [
      [1; 2; 3]
      [4; 5; 6]
      [7; 8; 9]
    ]
    let expected = [
      [7; 4; 1]
      [8; 5; 2]
      [9; 6; 3]
    ]
    let actual = Transforms.rotateRight input
    Assert.AreEqual(expected, actual)
    
  [<TestMethod>]
  member _.TestHeadRows () =
    let input = [
      [1; 2]
      [3; 4]
    ]
    let expected = (
      [3; 1],      
      [
        [2];
        [4]
      ]
    )
    let actual = Transforms.headRows input
    Assert.AreEqual(expected, actual)
  
  [<TestMethod>]
  member _.TestHeadRowsOnEmptyInput () =
    let input: int list list = []
    let expected = (
      [],      
      []
    )
    let actual = Transforms.headRows input
    Assert.AreEqual(expected, actual)
