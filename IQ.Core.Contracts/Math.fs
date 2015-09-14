// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Math.Contracts

open System
open System.Numerics

type Range<'T> = Range of MinValue : 'T * MaxValue : 'T    

/// <summary>
/// Defines contract for primtive, non-parametrized calculator
/// </summary>
type ICalculator =
    abstract Add:'T*'T->'T
    abstract Subtract:'T*'T->'T
    abstract Multiply:'T*'T->'T
    abstract Divide:'T*'T->'T
    abstract Zero:unit->'T
    abstract LessThan:'T*'T->bool
    abstract GreaterThan: 'T*'T->bool
    abstract LessThanOrEqual:'T*'T->bool
    abstract GreaterThanOrEqual: 'T*'T->bool

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


type IArrayCalculator =
    abstract Dot:'T[]*'T[]->'T    
    abstract Multiply: 'T[]*'T[]->'T[]
    abstract LessThan:('T seq)*'T->bool

type IArrayCalculator<'T> =
    abstract Dot:'T[]*'T[]->'T


type IStats =
    abstract runifd: count: int * min : 'T * max : 'T -> 'T[]
