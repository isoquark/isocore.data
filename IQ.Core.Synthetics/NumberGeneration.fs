namespace IQ.Core.Synthetics

open System
open System.Linq

open MathNet.Numerics.Distributions

open IQ.Core.Math

module internal Conversion =
    let inline cast<'TDst> (src : obj) = 
        src :?> 'TDst
    
    let inline convert<'TDst> (src : obj) = 
        Convert.ChangeType(src, typeof<'TDst>) |> cast<'TDst>
    
    let inline convertAll(items : 'TSrc seq) =
        seq {for item in items -> item |> convert<'TDst>}
    


/// <summary>
/// Implements basic random number generation capabilites
/// </summary>
/// <remarks>
/// Generating random numbers using this infrastructure will never be as fast as direct generation
/// because there will be a considerably amount of boxing and conversion taking place. It is
/// intended for use when clarity of expression/sematics or implementation efficiency is of
/// a hiher concern than runtime efficiency. If you want both implementation and runtime effeciency
/// for this sort of thing, then consider C++ ( ! ). Getting both in manged code is not so easy.
/// </remarks>
module internal NumberGeneration =              
    /// <summary>
    /// Helper to create a continuous uniform distribution that is characterized by a supplied range
    /// </summary>
    /// <param name="range">The range of the distribution</param>
    let cu(range :Range<'T>) =
        new ContinuousUniform( Conversion.convert(range.MinValue), Conversion.convert(range.MaxValue))
    
    /// <summary>
    /// Helper to create a discrete uniform distribution that is characterized by a supplied range
    /// </summary>
    /// <param name="range">The range of the distribution</param>
    let du(range : Range<'T>) =
        new DiscreteUniform( Conversion.convert(range.MinValue), Conversion.convert(range.MaxValue))

    /// <summary>
    /// Generates 8-bit signed integer values
    /// </summary>
    type Int8Generator(range) =
        let distribution = range |> du
        let nextValue() = distribution.Sample<int8>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray

        new() =
            Int8Generator(Range(SByte.MinValue, SByte.MaxValue))
            
        interface IValueGenerator<int8> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 8-bit unsigned integer values
    /// </summary>
    type UInt8Generator(range) =
        let distribution = range |> du
        let nextValue() = distribution.Sample<uint8>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray

        new() =
            UInt8Generator(Range(Byte.MinValue, Byte.MaxValue))
            

        interface IValueGenerator<uint8> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 16-bit signed integer values
    /// </summary>
    type Int16Generator(range) =
        let distribution = range |> du
        let nextValue() = distribution.Sample<int16>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray
                
        new () =
            Int16Generator(Range(Int16.MinValue, Int16.MaxValue))
        

        interface IValueGenerator<int16> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 16-bit unsigned integer values
    /// </summary>
    type UInt16Generator(range) =   
        let distribution = range |> du
        let nextValue() = distribution.Sample<uint16>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray
                
        new () =
            UInt16Generator(Range(UInt16.MinValue,UInt16.MaxValue))

        
        interface IValueGenerator<uint16> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()


    /// <summary>
    /// Generates 32-bit signed integer values
    /// </summary>
    type Int32Generator(range) =   
        let distribution = range |> du
        let nextValue() = distribution.Sample()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray
                
        new () =
            Int32Generator(Range(-1000000,1000000))
        
        interface IValueGenerator<int32> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 32-bit unsigned integer values
    /// </summary>
    type UInt32Generator(range) =   
        let distribution = range |> cu
        let nextValue() = distribution.Sample<uint32>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray
                    
        new () =
            UInt32Generator(Range(0u,1000000u))
        
        interface IValueGenerator<uint32> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 64-bit signed integer values
    /// </summary>
    type Int64Generator(range) =
        let distribution = range |> cu
        let nextValue() = distribution.Sample<int64>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray

        new () =
            Int64Generator(Range(-1000000L,1000000L))
        
        interface IValueGenerator<int64> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 64-bit unsigned integer values
    /// </summary>
    type UInt64Generator(range) =
        let distribution = range |> cu
        let nextValue() = distribution.Sample<uint64>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray
        
        new () =
            UInt64Generator(Range(0UL,1000000UL))

        interface IValueGenerator<uint64> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()


    /// <summary>
    /// Generates single floating point values
    /// </summary>
    type Float32Generator(range) =
        let distribution = range |> cu
        let nextValue() = distribution.Sample<float32>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray

        new() =
            Float32Generator(Range(-1000000.0f,1000000.0f))            
            
        interface IValueGenerator<float32> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates double floating point values
    /// </summary>
    type Float64Generator(range) =
        let distribution = range |> cu
        let nextValue() = distribution.Sample()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray

        new() =
            Float64Generator(Range(-1000000.0,1000000.0))
                        
        interface IValueGenerator<float> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates decimal values
    /// </summary>       
    type DecimalGenerator(range) =
        let distribution = range |> cu
        let nextValue() = distribution.Sample<decimal>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray

        new() =
            DecimalGenerator(Range(-1000000m,1000000m))
                        
        interface IValueGenerator<decimal> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()


                                    
module NumberGenerator =
        
    let get<'T>(min : 'T, max : 'T) =
        ValueGenerators.GetGenerator<'T>(Range(min,max))
    
    let getDefault<'T>() =
        ValueGenerators.GetGenerator<'T>()
