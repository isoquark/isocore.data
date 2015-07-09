namespace IQ.Core.Framework.Test

open System

open IQ.Core.Framework

module Time =
    

    type Tests(ctx,log) =    
        inherit ProjectTestContainer(ctx,log)
    
        let getTransformer() =
            let q = TimeConversions.ModuleType.TypeName |> FindTypeByName |> FindTypeElement |> List.singleton
            let t : ITransformer = TransformerConfig(q, None) |> ctx.AppContext.Resolve 
            t.AsTyped()

        let tx = getTransformer()        

        [<Fact>]
        let ``Projected Date onto defined targets``() =            
            //Date => BclDateTime
            Date(2015, 5, 23)  |> tx.Transform  |> Claim.equal (BclDateTime(2015, 5, 23))
            //Date => DateTime
            Date(2015, 5, 23) |> tx.Transform |> Claim.equal(DateTime(2015, 5, 23, 0, 0, 0))

        [<Fact>]
        let ``Projected DateTime onto defined targets``() =
            //DateTime => BclDateTime
            DateTime(2015, 5, 23, 0,0,0) |> tx.Transform |> Claim.equal (BclDateTime(2015, 5, 23))
            DateTime(2015, 5, 23, 0,0,0) |> tx.Transform |> Claim.equal (Date(2015, 5, 23))

        [<Fact>]
        let ``Projected BclDateTime onto defined targets``()=                    
            //BclDateTime => Date
            BclDateTime(2015, 5, 23) |> tx.Transform |> Claim.equal (Date(2015, 5, 23))
            //BclDateTime => DateTime
            BclDateTime(2015, 5, 23, 3, 4, 5) |> tx.Transform |> Claim.equal (DateTime(2015, 5, 23, 3, 4, 5))


        