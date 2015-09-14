// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Math

open System
open System.Linq
open System.Linq.Expressions
open System.Reflection
open System.Collections
open System.Numerics

open IQ.Core.Framework



open CalcOps

module Calculator =                  
     
    let init() =
        try
            Ops<uint8>.Init()
            Ops<uint16>.Init()
            Ops<uint32>.Init()
            Ops<uint64>.Init()
            Ops<int8>.Init()
            Ops<int16>.Init()
            Ops<int32>.Init()
            Ops<int64>.Init()
            Ops<float32>.Init()
            Ops<float>.Init()
            Ops<decimal>.Init()
        with 
            e ->
                reraise()    
        
    let sequence start minval maxval count = 
        let eq = Ops.Equal
        let increment = Ops.Increment
        seq{
                let mutable curval = start
                let mutable curcount = 1
                while(curcount <= count) do
                    yield curval
                    if eq.Invoke(curval, maxval) then
                        curval <- minval
                    else
                        curval <- curval|> increment.Invoke
                    curcount <- curcount + 1
           }    
        

    let inline private castAdd (x : ^T) (y : obj) =
        x + (y :?> ^T)
               
    let addItems(items : seq<'T>) =
        let result =
            if typeof<'T> = typeof<double> then
                let mutable result = 0.0
                for item in items.Cast<double>() do
                    result <- result + item
                result :> obj
            else if typeof<'T> = typeof<int> then
                let mutable result = 0
                for item in items.Cast<int>() do
                    result <- result + item
                result :> obj
            else
                NotSupportedException(sprintf "I don't know how to add items of type %s" typeof<'T>.Name) |> raise
        result :?> 'T
                     
    let inline private castMultiply (a : obj) (b : obj) =
        (a :?> ^T) * (b :?> ^T)

    let inline private castMultiplySum (x : IEnumerable) (y : IEnumerable) =
        let ex = x.GetEnumerator()
        let ey = y.GetEnumerator()
        let mutable result = Unchecked.defaultof< ^T>
        while(ex.MoveNext()) do
            ey.MoveNext() |> ignore
            result <- result + castMultiply ex.Current ey.Current
        result
        
    let private iterate<'T> (x : IEnumerable) (y : IEnumerable) f =
        use ex = x.Cast<'T>().GetEnumerator()
        use ey = y.Cast<'T>().GetEnumerator()
        while(ex.MoveNext()) do
            ey.MoveNext() |> ignore
            f ex.Current ey.Current                

    let multiplySum(x : seq<'T>)(y : seq<'T>) =
        if typeof<'T> = typeof<double> then
            let mutable result = 0.0
            let inline f x y = 
                result <- result + x * y
            f |> iterate x y
            result :> obj :?> 'T
        else
            NotSupportedException() |> raise
    
               
    let private create<'T>() = 
        let add = Ops<'T>.Add
        let addChecked = Ops<'T>.AddChecked
        let subtract = Ops<'T>.Subtract
        let subtractChecked = Ops<'T>.SubtractChecked
        let multiply = Ops<'T>.Multiply
        let zero = Ops<'T>.Zero
        let increment = Ops<'T>.Increment
        let decrement = Ops<'T>.Decrement
        let divide = Ops<'T>.Divide
        let modulo = Ops<'T>.Modulo
        let minval = Ops<'T>.MinValue
        let maxval = Ops<'T>.MaxValue
        let lt = Ops<'T>.LessThan
        let gt = Ops<'T>.GreaterThan
        let lteq = Ops<'T>.LessThanOrEqual
        let gteq = Ops<'T>.GreaterThanOrEqual
        let eq = Ops<'T>.Equal
        
        {new ICalculator<'T> with
            member this.Add (x, y) = add.Invoke(x, y)
            member this.AddChecked(x, y) = addChecked.Invoke(x,y)
            member this.Subtract(x,y) = subtract.Invoke(x,y)
            member this.SubtractChecked(x,y) = subtractChecked.Invoke(x,y)
            member this.Multiply(x, y) = multiply.Invoke(x,y)
            member this.Increment x = increment.Invoke(x)
            member this.Decrement x = decrement.Invoke(x)
            member this.Divide (x,y) = divide.Invoke(x,y)
            member this.Modulo (x,y) = modulo.Invoke(x,y)
            member this.Add items = addItems items
            member this.Zero = zero
            member this.MinValue = minval
            member this.MaxValue = maxval
            member this.Equal(x,y) = eq.Invoke(x,y)
            member this.LessThan (x,y) = lt.Invoke(x,y)            
            member this.GreaterThan (x,y) = gt.Invoke(x,y)
            member this.LessThanOrEqual(x,y) = lteq.Invoke(x,y)
            member this.GreaterThanOrEqual(x,y) = gteq.Invoke(x,y)
            member this.Sequence (start, minval, maxval, count) = sequence start minval maxval count
        } :> obj

    let private calculators = 
        [
            typeof<int8>, create<int8>()
            typeof<int16>, create<int16>()
            typeof<int32>, create<int32>()
            typeof<int64>, create<int64>()
            
            typeof<uint8>, create<uint8>()
            typeof<uint16>, create<uint16>()
            typeof<uint32>, create<uint32>()
            typeof<uint64>, create<uint64>()
            
            typeof<float32>, create<float32>()
            typeof<float>, create<float>()
            typeof<decimal>, create<decimal>()

        ] |> dict    
    
    
    let specific<'T>() =
        calculators.[typeof<'T>] :?> ICalculator<'T>



    let universal() =
        {new ICalculator with
            
            
            member this.Add (x,y) =  add x y
            member this.Subtract(x,y) = subtract x y
            member this.Multiply(x,y) = multiply x y
            member this.Divide(x,y) = divide x y
            member this.Zero() = zero()
            member this.GreaterThan(x,y) = gt x y
            member this.LessThan(x,y) = lt x y   
            member this.GreaterThanOrEqual(x, y) = gteq x y
            member this.LessThanOrEqual(x,y) = lteq x y
        }                   
        
    


type Number<'T>(value : 'T) =
    struct
        member this.Value = value
        static member val Ops : ICalculator<'T> = Calculator.specific()          
        static member MinValue : 'T = Ops.MinValue
        static member MaxValue : 'T = Ops.MaxValue
        static member Zero : 'T = Ops.Zero
        static member inline (+) (x : Number<_>, y: Number<_>) = Number<_>.Ops.Add(x.Value, y.Value) |> Number<_>           
        static member inline (-) (x : Number<_>, y: Number<_>) = Number<_>.Ops.Subtract(x.Value, y.Value) |> Number<_>           
    end

module VectorCalcs = 
    let inline multiply (x : 'T[]) (y : 'T[]) =
        let result = Array.zeroCreate<'T>(x.Length)
        for i in 0..x.Length-1 do
            result.[i] <- CalcOps.multiply x.[i] y.[i]
        result 

    let inline sumComponents (x : 'T[]) =
        let mutable result = Number<'T>.Zero
        for i in 0..x.Length-1 do
            result <-  CalcOps.add result x.[i]
        result
        
    let inline dot (x : 'T[]) (y : 'T[]) =
        multiply x y |> sumComponents
    

module ArrayCalculator =
    
    let multiply (x : 'T[]) (y : 'T[]) =
        if x.Length <> y.Length then
            ArgumentException("Arrays must be of the same length") |> raise
        
        let inline multiplyAt idx =
            y.[idx] |> CalcOps.multiply x.[idx]
        
        Array.init x.Length multiplyAt
                    
    
    let get() =
        {
            new IArrayCalculator with
                member this.Multiply(x,y) = y |> multiply x
                member this.Dot(x,y) = VectorCalcs.dot x y
                member this.LessThan(items : seq<'T>, y : 'T) = items |> Seq.exists(fun item -> not( lt item y))

        }       
        
