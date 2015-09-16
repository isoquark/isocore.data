// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Synthetics

open System
open System.Threading
open System.Collections
open System.Collections.Generic
open System.Linq

open IQ.Core.Framework
open IQ.Core.Data
open IQ.Core.Math

[<AutoOpen>]
module OrderedSequenceVocabulary =
        
    /// <summary>
    /// Specifies sequence generation settings
    /// </summary>
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


    /// <summary>
    /// Specifies sequence generation settings
    /// </summary>
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

    /// <summary>
    /// Specifies sequence generation settings
    /// </summary>
    type OrderedSequenceConfig<'T when 'T : comparison> = {
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
        Increment: 'T        
        /// Whether the sequence will start over when the maximum value is reached
        Cycle : bool
    }

    type EndOfSequenceException() =
        inherit Exception()




module OrderedSequenceProvider = 


    let private toGenericConfig<'T when 'T : comparison>(config : OrderedSequenceConfig) =
        {
            Name = config.Name
            ItemDataKind = config.ItemDataKind
            MinValue = config.MinValue.Unwrap<'T>()
            MaxValue = config.MaxValue.Unwrap<'T>()
            InitialValue = config.InitialValue.Unwrap<'T>()
            Increment = config.Increment.Unwrap<'T>()
            Cycle = config.Cycle
        }


    let inline private getNextValue( e : IEnumerator<'T>) =
        if e.MoveNext() |> not then
            EndOfSequenceException() |> raise
        e.Current            

    let inline private getNextRange count (e : IEnumerator<'T>) =
        seq{for i in 1..count do
                if e.MoveNext() |> not then
                    EndOfSequenceException() |> raise
                yield e.Current
        }

    type private Int64Sequence(config : OrderedSequenceConfig<int64>)=
        let e = ArithmeticEnumerator.createInline config.InitialValue config.MinValue config.Increment config.MaxValue config.Cycle
        let getNextValue() = e |> getNextValue
        let getNextRange count =e |> getNextRange count
        
        interface ISequenceProvider<int64> with
            member this.NextValue() = getNextValue()
            member this.NextRange count = count |> getNextRange               
        interface ISequenceProvider with
            member this.NextValue<'T when 'T : comparison>() =  getNextValue() :> obj :?> 'T
            member this.NextRange<'T when 'T : comparison>(count) = (getNextRange count).Cast<'T>() 

    type private SequenceProvider<'T when 'T : comparison>(config : OrderedSequenceConfig<'T>) =
        let e = ArithmeticEnumerator.createGeneric config.InitialValue config.MinValue config.Increment config.MaxValue config.Cycle        
        interface ISequenceProvider<'T> with
            member this.NextValue() = e |> getNextValue
            member this.NextRange count = e |> getNextRange count        
        interface ISequenceProvider with
            member this.NextValue<'X when 'X : comparison>() =  e |> getNextValue :> obj :?> 'X
            member this.NextRange<'X when 'X : comparison>(count) = ( e |> getNextRange count).Cast<'X>() 
        
    let private getProvider<'T when 'T : comparison>(config : OrderedSequenceConfig) =
        {
            Name = config.Name
            ItemDataKind = config.ItemDataKind
            MinValue = config.MinValue.Unwrap<'T>()
            MaxValue = config.MaxValue.Unwrap<'T>()
            InitialValue = config.InitialValue.Unwrap<'T>()
            Increment = config.Increment.Unwrap<'T>()
            Cycle = config.Cycle
        } |> SequenceProvider
                

    let private createSequence (config : OrderedSequenceConfig) =
        match config.ItemDataKind with
        | DataKind.UInt8 ->
            config |> getProvider<uint8> :> ISequenceProvider
        | DataKind.UInt16 ->
            config |> getProvider<uint16> :> ISequenceProvider
        | DataKind.UInt32 ->
            config |> getProvider<uint32> :> ISequenceProvider
        | DataKind.UInt64 ->
            config |> getProvider<uint64> :> ISequenceProvider
        | DataKind.Int8 ->
            config |> getProvider<int8> :> ISequenceProvider
        | DataKind.Int16 ->
            config |> getProvider<int16> :> ISequenceProvider
        | DataKind.Int32 ->
            config |> getProvider<int32> :> ISequenceProvider
        | DataKind.Int64 ->
            Int64Sequence(config |> toGenericConfig<int64>) :> ISequenceProvider       
        | DataKind.DateTime ->
            nosupport()
        | DataKind.TimeOfDay ->
            nosupport()
        | DataKind.Date ->
            nosupport()
        | DataKind.Duration ->
            nosupport()
        | DataKind.Float32 ->
            config |> getProvider<float32> :> ISequenceProvider
        | DataKind.Float64 ->
            config |> getProvider<float> :> ISequenceProvider
        | DataKind.Decimal ->
            config |> getProvider<decimal> :> ISequenceProvider
        | DataKind.Money ->
            nosupport()
        | _ -> nosupport()
         
    
    let get0<'T when 'T : comparison>(config : OrderedSequenceConfig) =
        config |> createSequence :> obj :?> ISequenceProvider<'T>

    let get1<'T when 'T : comparison>(config : OrderedSequenceConfig<'T>) =
        config |> SequenceProvider :> ISequenceProvider<'T>
        


