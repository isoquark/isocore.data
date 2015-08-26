﻿// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Math

open System
open System.Numerics

[<AutoOpen>]
module Contracts = 
    type Vector<'T> = Vector of Components : 'T[]

    type ICalculator =
        abstract Add:'T*'T->'T
        abstract Subtract:'T*'T->'T
        abstract Multiply:'T*'T->'T
        abstract Divide:'T*'T->'T
        abstract Zero:unit->'T

    type IArrayCalculator =
        abstract Multiply: 'T[]*'T[]->'T[]


    type ICalculator<'T> =
        abstract Add: 'T*'T->'T
        abstract Add: 'T seq -> 'T    
        abstract AddChecked: 'T*'T->'T    
    
        abstract Subtract: 'T*'T->'T
        abstract SubtractChecked: 'T*'T->'T    

        abstract Multiply: 'T*'T->'T
        abstract Divide: 'T*'T->'T
        abstract Modulo: 'T*'T->'T
        abstract Increment: 'T->'T
        abstract Decrement: 'T->'T
        abstract Sequence: start : 'T * minval : 'T * maxval : 'T *count : int -> 'T seq
        abstract Equal: 'T*'T->bool
        abstract LessThan: 'T*'T->bool
        abstract LessThanOrEqual: 'T*'T->bool
        abstract GreaterThan: 'T*'T->bool
        abstract GreaterThanOrEqual: 'T*'T->bool    
        abstract Zero:'T
        abstract MinValue : 'T
        abstract MaxValue : 'T

    type IVectorCalculator =
        abstract Dot:Vector<'T>*Vector<'T>->'T    

    type IVectorCalculator<'T> =
        abstract Dot:Vector<'T>*Vector<'T>->'T


    type IStats =
        abstract runifd: count: int * min : 'T * max : 'T -> 'T[]
