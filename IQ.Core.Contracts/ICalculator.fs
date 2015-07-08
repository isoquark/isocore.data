namespace IQ.Core.Contracts

type ICalculator =
    abstract Add:'T->'T->'T
    abstract Zero:unit->'T

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
    abstract Sequence: start : 'T * count : int -> 'T seq
    abstract Equal: 'T*'T->bool
    abstract LessThan: 'T*'T->bool
    abstract LessThanOrEqual: 'T*'T->bool
    abstract GreaterThan: 'T*'T->bool
    abstract GreaterThanOrEqual: 'T*'T->bool    
    abstract Zero:'T
    abstract MinValue : 'T
    abstract MaxValue : 'T
