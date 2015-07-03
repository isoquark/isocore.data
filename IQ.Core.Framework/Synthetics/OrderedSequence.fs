namespace IQ.Core.Synthetics

open System
open System.Threading
open System.Collections
open System.Collections.Generic
open System.Linq

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

    type EndOfSequenceException() =
        inherit Exception()

module OrderedSequenceConfig =
    let format (c : OrderedSequenceConfig) =
        ""


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


    let inline createEnumerator (initial : ^T) (min : ^T) (inc : ^S) (max : ^T) cycle = 
        let s = seq{ 
               let mutable cur =  initial
               while (cur < max) do
                    yield cur
                    cur <- cur + inc  
                    if cur = max then
                        yield cur
                        if cycle then
                            cur <- min
                
           }
        s.GetEnumerator()

    let private getNextValue( e : IEnumerator<'T>) =
        if e.MoveNext() |> not then
            EndOfSequenceException() |> raise
        e.Current            
        
    let private getNextRange count ( e : IEnumerator<'T>) =
        seq{for i in 1..count -> e |> getNextValue}        

    type private UInt8Sequence(config : OrderedSequenceConfig<uint8,uint8>) =
        let e = createEnumerator config.InitialValue config.MinValue config.Increment config.MaxValue config.Cycle
        let getNextValue() = e |> getNextValue
        let getNextRange count =e |> getNextRange count
        
        interface ISequence<uint8> with
            member this.NextValue() = getNextValue()
            member this.NextRange count = count |> getNextRange               
        interface ISequence with
            member this.NextValue<'T when 'T : comparison>() =  getNextValue() :> obj :?> 'T
            member this.NextRange<'T when 'T : comparison>(count) = (getNextRange count).Cast<'T>() 
                               
    type private Int32Sequence(config : OrderedSequenceConfig<int,int>) =
        let e = createEnumerator config.InitialValue config.MinValue config.Increment config.MaxValue config.Cycle
        let getNextValue() = e |> getNextValue
        let getNextRange count =e |> getNextRange count
        
        interface ISequence<int> with
            member this.NextValue() = getNextValue()
            member this.NextRange count = count |> getNextRange               
        interface ISequence with
            member this.NextValue<'T when 'T : comparison>() =  getNextValue() :> obj :?> 'T
            member this.NextRange<'T when 'T : comparison>(count) = (getNextRange count).Cast<'T>() 

    type private Int64Sequence(config : OrderedSequenceConfig<int64,int64>)=
        let e = createEnumerator config.InitialValue config.MinValue config.Increment config.MaxValue config.Cycle
        let getNextValue() = e |> getNextValue
        let getNextRange count =e |> getNextRange count
        
        interface ISequence<int64> with
            member this.NextValue() = getNextValue()
            member this.NextRange count = count |> getNextRange               
        interface ISequence with
            member this.NextValue<'T when 'T : comparison>() =  getNextValue() :> obj :?> 'T
            member this.NextRange<'T when 'T : comparison>(count) = (getNextRange count).Cast<'T>() 

    
        

    let private createSequence (config : OrderedSequenceConfig) =
        match config.ItemDataKind with
        | DataKind.UInt8 ->
            UInt8Sequence(config |> toGenericConfig1<uint8>) :> ISequence       
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
            Int32Sequence(config |> toGenericConfig1<int32>) :> ISequence       
        | DataKind.Int64 ->
            Int64Sequence(config |> toGenericConfig1<int64>) :> ISequence       
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
        config |> createSequence :> obj :?> ISequence<'T>



