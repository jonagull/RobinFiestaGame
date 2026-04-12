module Sim.Machine

open Sim.Shapes

type public Machine =
  abstract member WorkShape: Shape -> unit