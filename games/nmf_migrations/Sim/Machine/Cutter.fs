module Sim.Cutter

open Sim.Shapes

type Cutter(cut: Cut) =
  member _.CutShape (shape: Shape) :  Shape list =
    shape.Cut cut