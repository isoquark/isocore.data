namespace IQ.Core.Synthetics

open System
open System.Linq

open MathNet.Numerics.Random
open MathNet.Numerics.Distributions

open IQ.Core.Math



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
    /// The default seed provider
    /// </summary>
    let seed() =
        RandomSeed.Robust()    
    
    /// <summary>
    /// Helper to create a continuous uniform distribution that is characterized by a supplied range and a seed value
    /// </summary>
    /// <param name="range">The range of the distribution</param>
    /// <param name="seed">The seed value used for the random source</param>
    let cuseed seed (range : Range<'T>) =
        new ContinuousUniform(SmartConvert.convertT(range.MinValue), SmartConvert.convertT(range.MaxValue), SystemRandomSource(seed, true))

    /// <summary>
    /// Helper to create a continuous uniform distribution that is characterized by a supplied range
    /// </summary>
    /// <param name="range">The range of the distribution</param>
    let cu(range :Range<'T>) =
        range |> cuseed (seed())

    /// <summary>
    /// Helper to create a discrete uniform distribution that is characterized by a supplied range and a seed value
    /// </summary>
    /// <param name="range">The range of the distribution</param>
    /// <param name="seed">The seed value used for the random source</param>
    let duseed seed (range : Range<'T>) =
        new DiscreteUniform(SmartConvert.convertT(range.MinValue), SmartConvert.convertT(range.MaxValue), SystemRandomSource(seed, true))
    
    /// <summary>
    /// Helper to create a discrete uniform distribution that is characterized by a supplied range
    /// </summary>
    /// <param name="range">The range of the distribution</param>
    let du(range : Range<'T>) =
        range |> duseed (seed())

    /// <summary>
    /// Generates 8-bit signed integer values
    /// </summary>
    type Int8Generator(range, seed) =
        let distribution =  range |> duseed seed
        let nextValue() = distribution.Sample<int8>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray

        new(min : int8, max) =
            Int8Generator(Range(min,max), seed())

        new(min, max, seed) =
            Int8Generator(Range(min, max), seed)


        new() =
            Int8Generator(Range(SByte.MinValue, SByte.MaxValue), seed())
            
        interface IValueGenerator<int8> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 8-bit unsigned integer values
    /// </summary>
    type UInt8Generator(range, seed) =
        let distribution =  range |> duseed seed
        let nextValue() = distribution.Sample<uint8>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray

        new(min : uint8, max) =
            UInt8Generator(Range(min,max), seed())

        new(min, max, seed) =
            UInt8Generator(Range(min, max), seed)

        new() =
            UInt8Generator(Range(Byte.MinValue, Byte.MaxValue), seed())
            

        interface IValueGenerator<uint8> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 16-bit signed integer values
    /// </summary>
    type Int16Generator(range, seed) =
        let distribution =  range |> duseed seed
        let nextValue() = distribution.Sample<int16>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray
                
        new(min : int16, max) =
            Int16Generator(Range(min,max), seed())

        new(min, max, seed) =
            Int16Generator(Range(min, max), seed)

        new () =
            Int16Generator(Range(Int16.MinValue, Int16.MaxValue), seed())
        

        interface IValueGenerator<int16> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 16-bit unsigned integer values
    /// </summary>
    type UInt16Generator(range, seed) =   
        let distribution =  range |> duseed seed
        let nextValue() = distribution.Sample<uint16>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray
                
        new(min : uint16, max) =
            UInt16Generator(Range(min,max), seed())

        new(min, max, seed) =
            UInt16Generator(Range(min, max), seed)

        new () =
            UInt16Generator(Range(UInt16.MinValue,UInt16.MaxValue), seed())

        
        interface IValueGenerator<uint16> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()


    /// <summary>
    /// Generates 32-bit signed integer values
    /// </summary>
    type Int32Generator(range, seed) =   
        let distribution =  range |> duseed seed
        let nextValue() = distribution.Sample()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray
                
        new(min : int32, max) =
            Int32Generator(Range(min,max), seed())

        new(min, max, seed) =
            Int32Generator(Range(min, max), seed)
        
        new () =
            Int32Generator(Range(-1000000,1000000), seed())
        
        interface IValueGenerator<int32> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 32-bit unsigned integer values
    /// </summary>
    type UInt32Generator(range, seed) =   
        let distribution =  range |> cuseed seed
        let nextValue() = distribution.Sample<uint32>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray
                    
        new(min : uint32, max) =
            UInt32Generator(Range(min,max), seed())

        new(min, max, seed) =
            UInt32Generator(Range(min, max), seed)
        
        new () =
            UInt32Generator(Range(0u,1000000u), seed())
        
        interface IValueGenerator<uint32> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 64-bit signed integer values
    /// </summary>
    type Int64Generator(range, seed) =
        let distribution =  range |> cuseed seed
        let nextValue() = distribution.Sample<int64>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray

        new(min : int64, max) =
            Int64Generator(Range(min,max), seed())

        new(min, max, seed) =
            Int64Generator(Range(min, max), seed)

        new () =
            Int64Generator(Range(-1000000L,1000000L), seed())
        


        interface IValueGenerator<int64> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates 64-bit unsigned integer values
    /// </summary>
    type UInt64Generator(range, seed) =
        let distribution =  range |> cuseed seed
        let nextValue() = distribution.Sample<uint64>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray
        
        new(min : uint64, max) =
            UInt64Generator(Range(min,max), seed())

        new(min, max, seed) =
            UInt64Generator(Range(min, max), seed)

        new () =
            UInt64Generator(Range(0UL,1000000UL), seed())

        
        interface IValueGenerator<uint64> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()


    /// <summary>
    /// Generates single floating point values
    /// </summary>
    type Float32Generator(range, seed) =
        let distribution =  range |> cuseed seed
        let nextValue() = distribution.Sample<float32>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray

        new(min : float32, max) =
            Float32Generator(Range(min,max), seed())

        new(min, max, seed) =
            Float32Generator(Range(min, max), seed)

        new() =
            Float32Generator(Range(-1000000.0f,1000000.0f), seed())            
            

        interface IValueGenerator<float32> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates double floating point values
    /// </summary>
    type Float64Generator(range, seed) =
        let distribution =  range |> cuseed seed
        let nextValue() = distribution.Sample()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray


        new(min : float, max) =
            Float64Generator(Range(min,max), seed())

        new(min, max, seed) =
            Float64Generator(Range(min, max), seed)

        new() =
            Float64Generator(Range(-1000000.0,1000000.0), seed())
                        
        interface IValueGenerator<float> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()

    /// <summary>
    /// Generates decimal values
    /// </summary>       
    type DecimalGenerator(range, seed) =
        let distribution =  range |> cuseed seed
        let nextValue() = distribution.Sample<decimal>()
        let nextValues(count) = [| for i in 1..count -> nextValue()|] |> Seq.ofArray

        new(min : decimal, max) =
            DecimalGenerator(Range(min,max), seed())

        new(min, max, seed) =
            DecimalGenerator(Range(min, max), seed)
            


        new() =
            DecimalGenerator(Range(-1000000m,1000000m), seed())
                        
        interface IValueGenerator<decimal> with
            member this.NextValue() = nextValue()
            member this.NextValue() = nextValue() :> obj       
            member this.NextValues(count) = nextValues(count)
            member this.NextValues(count) = nextValues(count).Cast<obj>()


                                    
module NumberGenerator =
        
    let withSeed<'T>(min : 'T, max : 'T, seed) =
        ValueGenerators.GetGenerator<'T>(Range(min,max), seed)

    let withRange<'T>(min : 'T, max) =
        ValueGenerators.GetGenerator<'T>(Range(min,max), NumberGeneration.seed())
    
    let standard<'T>() =
        ValueGenerators.GetGenerator<'T>()

    /// <summary>
    /// Gets the default realization of <see cref="INumberGeneratorProvider"/>
    /// </summary>
    let defaultProvider() =
        {new INumberGeneratorProvider with
            member this.GetGenerator() = standard()
            member this.GetGenerator(min,max) = withRange(min,max)
            member this.GetGenerator(min,max, seed) = withSeed(min,max,seed)
        }