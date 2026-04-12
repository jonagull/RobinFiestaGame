module Sim.Transformator

open Sim.Shapes
open Sim.Machine

type Transformator(trans: Transformation) =
  interface Machine with
    member _.WorkShape shape =
      shape.Rotate trans