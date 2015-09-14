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

module CalcOps = 
    type Ops<'T>() =
        static member Init() = ()
        
        static member val Add : BinaryFunc<'T> = 
            ExpressionBuilder.binaryLambda Expression.Add |> ExpressionBuilder.compile 
        
        static member val AddChecked : BinaryFunc<'T> = 
            ExpressionBuilder.binaryLambda Expression.AddChecked |> ExpressionBuilder.compile    
        
        static member val Subtract : BinaryFunc<'T> = 
            ExpressionBuilder.binaryLambda Expression.Subtract |> ExpressionBuilder.compile 
        
        static member val SubtractChecked : BinaryFunc<'T> = 
            ExpressionBuilder.binaryLambda Expression.SubtractChecked |> ExpressionBuilder.compile 
        
        static member val Multiply  : BinaryFunc<'T> = 
            ExpressionBuilder.binaryLambda Expression.Multiply |> ExpressionBuilder.compile         
        
        static member val MultiplyChecked : BinaryFunc<'T> = 
            ExpressionBuilder.binaryLambda Expression.MultiplyChecked |> ExpressionBuilder.compile 
        
        static member val Divide : BinaryFunc<'T> = 
            ExpressionBuilder.binaryLambda Expression.Divide |> ExpressionBuilder.compile 
        
        static member val Modulo : BinaryFunc<'T> = 
            ExpressionBuilder.binaryLambda Expression.Modulo |> ExpressionBuilder.compile 
        
        static member val Increment : UnaryFunc<'T> = 
            ExpressionBuilder.unaryLambda Expression.Increment |> ExpressionBuilder.compile 
        
        static member val Decrement : UnaryFunc<'T> = 
            ExpressionBuilder.unaryLambda Expression.Decrement |> ExpressionBuilder.compile 
        
        static member val Equal : BinaryPredicate<'T> = 
            ExpressionBuilder.binaryLambda Expression.Equal |> ExpressionBuilder.compile 
        
        static member val LessThan : BinaryPredicate<'T> = 
            ExpressionBuilder.binaryLambda Expression.LessThan |> ExpressionBuilder.compile 
        
        static member val LessThanOrEqual : BinaryPredicate<'T> = 
            ExpressionBuilder.binaryLambda Expression.LessThanOrEqual |> ExpressionBuilder.compile 
        
        static member val GreaterThan : BinaryPredicate<'T> = 
            ExpressionBuilder.binaryLambda Expression.GreaterThan |> ExpressionBuilder.compile 
        
        static member val GreaterThanOrEqual : BinaryPredicate<'T> = 
            ExpressionBuilder.binaryLambda Expression.GreaterThanOrEqual |> ExpressionBuilder.compile 
        
        static member val Zero : 'T = 
            Unchecked.defaultof<'T>
        
        static member val MaxValue : 'T = 
            typeof<'T>.GetField("MaxValue", BindingFlags.Public ||| BindingFlags.Static).GetValue(null) :?> 'T    
        
        static member val MinValue : 'T = 
            typeof<'T>.GetField("MinValue", BindingFlags.Public ||| BindingFlags.Static).GetValue(null) :?> 'T    


    let inline add x y =  Ops.Add.Invoke(x,y)
    
    let inline addChecked x y = Ops.Add.Invoke(x,y)

    let inline subtract x y = Ops.Subtract.Invoke(x,y)
    
    let inline subtractChecked x y = Ops.SubtractChecked.Invoke(x,y)
    
    let inline multiply x y = Ops.Multiply.Invoke(x,y)
    
    let inline multiplyChecked x y = Ops.MultiplyChecked.Invoke(x,y)

    let inline divide x y  = Ops.Divide.Invoke(x,y)
    
    let inline zero() = Ops.Zero    
   
    let inline increment x = Ops.Increment.Invoke(x)
    
    let inline decrement x = Ops.Decrement.Invoke(x)

    let inline lt x y = Ops.LessThan.Invoke(x,y)

    let inline gt x y = Ops.GreaterThan.Invoke(x,y)


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
    
    
    let get<'T>() =
        calculators.[typeof<'T>] :?> ICalculator<'T>



    let get1() =
        {new ICalculator with
            
            member this.Add (x,y) =  add x y
            member this.Subtract(x,y) = subtract x y
            member this.Multiply(x,y) = multiply x y
            member this.Divide(x,y) = divide x y
            member this.Zero() = zero()
            member this.GreaterThan(x,y) = gt x y
            member this.LessThan(x : 'T,y : 'T) = lt x y
        }                   
        
    


type Number<'T>(value : 'T) =
    struct
        member this.Value = value
        static member val Ops : ICalculator<'T> = Calculator.get()          
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
        
