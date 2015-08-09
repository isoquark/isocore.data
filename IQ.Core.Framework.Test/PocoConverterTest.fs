// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework.Test

open System
open System.Collections
open System.Collections.Generic


open IQ.Core.Framework

module PocoConverter =
    type DataEntityA()  =
        member val Field1 = 0 with get,set
        member val Field2 = "" with get,set
        member val Field3 = 5m with get,set
        member val Field4 = Nullable(4) with get, set 
            
        override  this.Equals(other) =
                let x = this
                let y = other :?> DataEntityA
                x.Field1 = y.Field1 && x.Field2 = y.Field2 && x.Field3 = y.Field3
        
        override this.GetHashCode() =
            let mutable hash = 17
            hash <- hash*23 + this.Field1.GetHashCode()
            hash <- hash*23 + this.Field2.GetHashCode()
            hash <- hash*23 + this.Field3.GetHashCode()
            hash;
            
    
    type Tests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)

    
        let converter = ctx.ClrMetadataProvider |> PocoConverterConfig |>  PocoConverter.get

        [<Fact>]
        let ``Created data entity from value array``() =
            let expect = DataEntityA(Field1 = 12, Field2 = "13", Field3 = 14.0m, Field4 = Nullable(15))
            let actual = converter.FromValueArray([|12; "13"; 14.0m; Nullable(15)|], typeof<DataEntityA>) :?> DataEntityA
            Claim.equal expect actual

        [<Fact>]
        let ``Created value index from data entity``() =
            let entity = DataEntityA(Field1 = 12, Field2 = "13", Field3 = 14.0m, Field4 = Nullable(15))
            let expect = 
                ["Field1", 0, entity.Field1 :> obj
                 "Field2", 1, entity.Field2 :> obj
                 "Field3", 2, entity.Field3 :> obj
                 "Field4", 3, entity.Field4 :> obj
                ]
                |> ValueIndex.create
            
            let actual = entity |> converter.ToValueIndex
            Claim.equal expect actual            
            

        [<Fact>]
        let ``Created value array from data entity``() =
            let entity = DataEntityA(Field1 = 12, Field2 = "13", Field3 = 14.0m, Field4 = Nullable(15))
            let expect = [|12 :> obj; "13" :> obj; 14.0m :> obj; Nullable(15) :> obj|]
            let actual = entity |> converter.ToValueArray
            Claim.equal expect actual   
        
        [<Fact>]
        let ``Created data entity from value index``() =
            let expect = DataEntityA(Field1 = 12, Field2 = "13", Field3 = 14.0m)
            let values = 
                ["Field1", 0, expect.Field1 :> obj
                 "Field2", 1, expect.Field2 :> obj
                 "Field3", 2, expect.Field3 :> obj
                 "Field4", 3, expect.Field4 :> obj
                ]
                |> ValueIndex.create
            let actual = converter.FromValueIndex(values, typeof<DataEntityA>) :?> DataEntityA                
            Claim.equal expect actual   
