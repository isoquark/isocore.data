namespace IQ.Core.Synthetics

open System
open System.Linq
open System.Globalization

open MathNet.Numerics.Distributions

open IQ.Core.Math

module DateGeneration =
   

    /// <summary>
    /// Generates Date values
    /// </summary>       
    type DateGenerator(range: Range<BclDateTime>) =
        static let Calendar = GregorianCalendar.ReadOnly((GregorianCalendar()))

        let yGen = NumberGenerator.get<int32>(range.MinValue.Year, range.MaxValue.Year)
        let mGen = NumberGenerator.get<int32>(range.MinValue.Month, range.MaxValue.Month)
       
        let getNextValues(count, range : Range<BclDateTime>) =            
            [|for i in 1..count do
                let y = yGen.NextValue()
                let m = mGen.NextValue()
                let dGen = NumberGenerator.get<int32> (1, Calendar.GetDaysInMonth(y, m))
                let d = dGen.NextValue()
                yield DateTime(y, m, d)            
            |]
        
        new(min, max) =
            DateGenerator(Range(min, max))
        
        new () =
            DateGenerator(Range(BclDateTime(2013, 1, 1), BclDateTime(2016, 12, 31)))
        interface IValueGenerator<BclDateTime> with
        
            member this.NextValue() = getNextValues(1, range) |> Seq.exactlyOne
            member this.NextValue() = getNextValues(1, range) |> Seq.exactlyOne :> obj
            member this.NextValues(count) = getNextValues(count, range) |> Seq.ofArray
            member this.NextValues(count) = getNextValues(count, range).Cast<obj>()

    
module DateGenerator =
    let get(min : BclDateTime, max : BclDateTime) =
        ValueGenerators.GetGenerator<BclDateTime>(Range(min,max))

    let getDefault() =
        ValueGenerators.GetGenerator<BclDateTime>()        


