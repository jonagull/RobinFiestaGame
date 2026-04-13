module Sim.Cutter

open Sim.Shapes

type Cutter(cut: Cut) =
  member _.CutShape (shape: Shape) =
    shape.Cut cut