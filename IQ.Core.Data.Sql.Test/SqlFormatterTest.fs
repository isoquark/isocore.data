namespace IQ.Core.Data.Sql.Test

open System
open System.Data

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Sql
open IQ.Core.Framework


module SqlFormatter =
    [<Table("MySchema")>]
    type private RecordA = {
        ``Field 01`` : int
        ``Field 02`` : string
        ``Field 03`` : decimal
    }
    
    type Tests(ctx, log) = 
        inherit ProjectTestContainer(ctx,log)
  
        [<FactAttribute>]
        let ``Formatted BclDateTime value as SQL``() =
            let expect = "'2015-09-03 09:33:56.123'"
            let actual = BclDateTime(2015, 9,3, 9,33,56,123) |> SqlFormatter.formatValue
            actual |> Claim.equal expect

        [<FactAttribute>]
        let ``Formatted string value as SQL``() =
            let expect = "\'This test\'\'s purpose is to verify SQL string formatting\'"
            let actual = "This test's purpose is to verify SQL string formatting" |> SqlFormatter.formatValue
            actual |> Claim.equal expect

        [<FactAttribute>]
        let ``Formatted boolean value as SQL``() =
            Claim.equal "0" (false |> SqlFormatter.formatValue)
            Claim.equal "1" (true |> SqlFormatter.formatValue)

        [<FactAttribute>]
        let ``Formatted null as SQL``() =
            Claim.equal "null" (null |> SqlFormatter.formatValue)
            Claim.equal "null" (DBNull.Value |> SqlFormatter.formatValue)

        [<FactAttribute>]
        let ``Formatted Guid as SQL``() =
            let expect = "'41816141-0dbd-46c6-ab55-6037a1da8790'"
            let actual = Guid.Parse(expect |> Txt.removeChar ''') |> SqlFormatter.formatValue
            Claim.equal expect actual
    

        [<FactAttribute>]
        let ``Formatted tabular select statement``() =
            let expect = "select [Field 01],[Field 02],[Field 03] from [MySchema].[RecordA]"     
            let actual = SqlFormatter.formatTabularSelectT<RecordA>()
            actual |> Claim.equal expect


  