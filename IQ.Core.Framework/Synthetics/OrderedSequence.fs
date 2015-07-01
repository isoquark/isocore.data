namespace IQ.Core.Framework.Synthetics

open System
open System.Threading
open System.Collections.Generic

open IQ.Core.Framework
open IQ.Core.Data

[<AutoOpen>]
module OrderedSequenceVocabulary =
    
    type ISequence<'T when 'T : comparison> =
        abstract NextValue:unit->'T
        abstract NextRange:int->'T IEnumerable
    type ISequence =
        abstract NextValue:unit->'T when 'T : comparison
        abstract NextRange:int->'T IEnumerable when 'T : comparison

    
    type OrderedSequenceConfig = {
        /// Then name of the sequence
        Name : string
        /// The sequence item data type
        ItemDataKind : DataKind
        /// The minimum value generated
        MinValue : DataValue
        /// The maximimum value generated
        MaxValue : DataValue
        /// The value of the first item which must be within the range demarcated
        /// by MinValue and MaxValue
        InitialValue : DataValue
        /// The distance between two elements in the sequence
        Increment : DataValue        
        /// Whether the sequence will start over when the maximum value is reached
        Cycle : bool
    }


    type OrderedSequenceConfig<'T, 'S when 'T : comparison> = {
        /// Then name of the sequence
        Name : string
        /// The sequence item data type
        ItemDataKind : DataKind
        /// The minimum value generated
        MinValue : 'T
        /// The maximimum value generated
        MaxValue : 'T
        /// The value of the first item which must be within the range demarcated
        /// by MinValue and MaxValue
        InitialValue : 'T
        /// The distance between two elements in the sequence
        Increment: 'S        
        /// Whether the sequence will start over when the maximum value is reached
        Cycle : bool
    }

    type SequenceTerminatedException() =
        inherit Exception()

module OrderedSequence = 

    let private toGenericConfig1<'T when 'T : comparison>(config : OrderedSequenceConfig) =
        {
            Name = config.Name
            ItemDataKind = config.ItemDataKind
            MinValue = config.MinValue.Unwrap<'T>()
            MaxValue = config.MaxValue.Unwrap<'T>()
            InitialValue = config.InitialValue.Unwrap<'T>()
            Increment = config.Increment.Unwrap<'T>()
            Cycle = config.Cycle
        }

    let private toGenericConfig2<'T, 'S when 'T : comparison>(config : OrderedSequenceConfig) =
        {
            Name = config.Name
            ItemDataKind = config.ItemDataKind
            MinValue = config.MinValue.Unwrap<'T>()
            MaxValue = config.MaxValue.Unwrap<'T>()
            InitialValue = config.InitialValue.Unwrap<'T>()
            Increment = config.Increment.Unwrap<'S>()
            Cycle = config.Cycle
        }


    let inline private realize (initial : ^T) (min : ^T) (inc : ^S) (max : ^T) cycle = 
        seq{ 
               let mutable cur =  initial
               while (cur < max) do
                    yield cur
                    cur <- cur + inc  
                    if cur > max && cycle then
                        cur <- min                             
           }

    
    type private UInt8Sequence(config : OrderedSequenceConfig<uint8,uint8>) =
        let s = realize config.InitialValue config.MinValue config.Increment config.MaxValue config.Cycle
        let enumerator = s.GetEnumerator()
        let getNextValue() =
                if enumerator.MoveNext() |> not then
                    SequenceTerminatedException() |> raise
                enumerator.Current            
        let getNextRange count =
            seq{for i in 1..count -> getNextValue()}

        interface ISequence<uint8> with
            member this.NextValue() = getNextValue()
            member this.NextRange count = count |> getNextRange               
        interface ISequence with
            member this.NextValue<'T when 'T : comparison>() = getNextValue() :> obj :?> 'T
            member this.NextRange<'T when 'T : comparison>(count) = seq{for x in (count |> getNextRange) -> x :> obj :?> 'T}
             
                               
    let private createSequence (config : OrderedSequenceConfig) =
        match config.ItemDataKind with
        | DataKind.UInt8 ->
            let config = config |> toGenericConfig1<uint8>
            UInt8Sequence(config)        
        | DataKind.UInt16 ->
            nosupport()
        | DataKind.UInt32 ->
            nosupport()
        | DataKind.UInt64 ->
            nosupport()
        | DataKind.Int8 ->
            nosupport()
        | DataKind.Int16 ->
            nosupport()
        | DataKind.Int32 ->
            nosupport()
        | DataKind.Int64 ->
            nosupport()
        | DataKind.DateTime32 ->
            nosupport()
        | DataKind.DateTime64 ->
            nosupport()
        | DataKind.DateTime ->
            nosupport()
        | DataKind.TimeOfDay ->
            nosupport()
        | DataKind.Date ->
            nosupport()
        | DataKind.Timespan ->
            nosupport()
        | DataKind.Float32 ->
            nosupport()
        | DataKind.Float64 ->
            nosupport()
        | DataKind.Decimal ->
            nosupport()
        | DataKind.Money ->
            nosupport()
        | _ -> nosupport()
         

    
    let get<'T when 'T : comparison>(config : OrderedSequenceConfig) =
        {
            new ISequence<'T> with
                member this.NextValue() = nosupport()
                member this.NextRange count = nosupport()                   
        }
        



