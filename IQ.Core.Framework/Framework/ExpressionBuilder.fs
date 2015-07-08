namespace IQ.Core.Framework

open System
open System.Linq
open System.Linq.Expressions




/// <summary>
/// Defines streamlined vocabulary for building dynamic LINQ expressions
/// </summary>
module ExpressionBuilder =

    let convert<'T> (e : Expression) = 
        Expression.Convert(e, typeof<'T>)
    
    let lambda<'TDelegate> (parameters : ParameterExpression seq) (body : Expression) =
        Expression.Lambda<'TDelegate>(body, parameters)
    
    let compile<'TDelegate> (e : Expression<'TDelegate>) =
        e.Compile()
    
    let parameter<'T> name =
        Expression.Parameter(typeof<'T>, name)
       

    /// <summary>
    /// Creates a binary lambda epxression
    /// </summary>
    /// <param name="f">Function that takes 2 expressions and yields binary expression</param>
    let binaryLambda<'T,'TResult> (f : Expression*Expression->BinaryExpression) =
        try
            let p0, p1 = ("p0" |> parameter<'T>, "p1" |> parameter<'T>)
            
            if typeof<'T> = typeof<uint8> || typeof<'T> = typeof<int8> then            
                ( p0 |> convert<int>, p1 |> convert<int>) |> f  |> convert<'TResult> |> lambda<Func<'T, 'T, 'TResult>> [p0; p1]  
            else
                (p0, p1) |> f  |> lambda<Func<'T, 'T, 'TResult>> [p0; p1]
        with
            e ->
                reraise()

    let private wrapUnaryConversion<'TSrc,'TDst>  (f : Expression->UnaryExpression) (e : Expression) =
        e |> convert<'TSrc> |> f |> convert<'TDst>

    /// <summary>
    /// Creates a unary lambda epxression
    /// </summary>
    /// <param name="f">Function that takes an expression and yields a unary expression</param>
    let unaryLambda<'T, 'TResult> (f : Expression->UnaryExpression) =
        try
            let p0 = "p0" |> parameter<'T>
            if typeof<'T> = typeof<uint8> then
                p0 |> convert<uint16> |> f |> convert<'TResult> |> lambda<Func<'T, 'TResult>>[p0] 
            else if typeof<'T> = typeof<int8> then
                p0 |> convert<int16> |> f |> convert<'TResult> |> lambda<Func<'T, 'TResult>>[p0] 
            else
                p0 |> f |> lambda<Func<'T,'TResult>> [p0]
        with
            e ->
                reraise()
