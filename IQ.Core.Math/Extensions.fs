namespace IQ.Core.Math

open System
open System.Runtime.CompilerServices
open MathNet.Numerics.Distributions

[<AutoOpen>]
module MathExtensions =

    type Range<'T>
    with
        member this.MinValue = match this with Range(MinValue=x) -> x
        member this.MaxValue = match this with Range(MaxValue=x) -> x
            
    type IContinuousDistribution
    with
        member this.Sample<'T>() =
            Convert.ChangeType(this.Sample(), typeof<'T>) :?> 'T

    type IDiscreteDistribution
    with
        member this.Sample<'T>() =
            Convert.ChangeType(this.Sample(), typeof<'T>) :?> 'T       
    
