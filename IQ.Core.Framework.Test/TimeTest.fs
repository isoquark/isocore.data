namespace IQ.Core.Framework.Test

open System



type TimeTests(ctx,log) =    
    inherit ProjectTestContainer(ctx,log)
    
    let getTransformer() =
        let q = TimeConversions.ModuleType.TypeName |> FindTypeByName |> FindTypeElement |> List.singleton
        let t : ITransformer = TransformerConfig(q, None) |> ctx.AppContext.Resolve 
        t.AsTyped()

    let transformer = getTransformer()

    

    [<Fact>]
    let ``Discovered Time Transformations``() =        
        transformer.CanTransform<DateTime,BclDateTime>() |> Claim.isTrue
        transformer.CanTransform<BclDateTime,DateTime>() |> Claim.isTrue
        
        