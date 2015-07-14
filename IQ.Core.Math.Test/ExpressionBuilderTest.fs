namespace IQ.Core.Math.Test

open System
open System.Collections.Generic

open System.Linq.Expressions

open IQ.Core.Math

//Note, this is just for testing - would be far too slow otherwise!
[<AutoOpen>]
module CalcOps =
    let add<'T> =
        ExpressionBuilder.binaryLambda<'T,'T> Expression.Add  |> ExpressionBuilder.compile  |> FSharpFunc.ToFSharpFunc
    let mul<'T> =
        ExpressionBuilder.binaryLambda<'T,'T> Expression.Multiply |> ExpressionBuilder.compile  |> FSharpFunc.ToFSharpFunc
    let inc<'T> =
        ExpressionBuilder.unaryLambda<'T,'T> Expression.Increment |> ExpressionBuilder.compile |> FSharpFunc.ToFSharpFunc
    let dec<'T> =
        ExpressionBuilder.unaryLambda<'T,'T> Expression.Decrement |> ExpressionBuilder.compile |> FSharpFunc.ToFSharpFunc
    
module ExpressionBuilder = 
    
        
    type LogicTests(ctx,log) = 
        inherit ProjectTestContainer(ctx,log) 
    
        [<Fact>]
        let ``Created binary lambda expressions``() =            
            add 5 10|> Claim.equal 15
            add 5uy 10uy |> Claim.equal 15uy            
            add 5s 10s |> Claim.equal 15s
            mul 10m 20m |> Claim.equal 200m
            

        [<Fact>]
        let ``Created unary lambda expressions``() =            
            5 |> inc |> Claim.equal 6
            5uy |> inc |> Claim.equal 6uy
            7.0 |> inc |> Claim.equal 8.0
            5 |> dec |> Claim.equal 4
            5uy |> dec |> Claim.equal 4uy
            7.0 |> dec |> Claim.equal 6.0
            


