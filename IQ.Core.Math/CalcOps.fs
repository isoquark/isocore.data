// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Math

open System
open System.Linq
open System.Linq.Expressions
open System.Reflection

module CalcOps = 
    type Ops<'T>() =
        
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

        static member Init() = ()


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

    let inline lteq x y = Ops.LessThanOrEqual.Invoke(x,y)

    let inline gteq x y = Ops.GreaterThanOrEqual.Invoke(x,y)


