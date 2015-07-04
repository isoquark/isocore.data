namespace IQ.Core.Data.Test

open IQ.Core.TestFramework

open System
open System.Data

open IQ.Core.Framework
open IQ.Core.Data


open IQ.Core.Framework.Test

module DataTable=
    
    type private DataTableRecord = {
        Field01 : int64
        Field02 : bool
        Field03 : string
    }

    type Tests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
        
        [<Fact>]
        let ``Created DataTable from record metadata - strongly typed``() =
            let dataTable = tabularproxy<DataTableRecord> |> TabularProxy |> DataTable.fromProxyDescription
            Claim.equal 3 dataTable.Columns.Count
            Claim.equal "Field01" dataTable.Columns.[0].ColumnName
            Claim.equal (typeof<int64>) dataTable.Columns.[0].DataType


        [<Fact>]
        let ``Created DataTable from list of record values - weakly typed``() =
            let recordValues = [
                {Field01 = 1000L; Field02 = true; Field03 = "ABC"} :> obj
                {Field01 = 1001L; Field02 = false; Field03 = "XYZ"} :> obj
                {Field01 = 1002L; Field02 = true; Field03 = "FGH"} :> obj
            ]

            let dataTable = recordValues |> DataTable.fromProxyValues (tabularproxy<DataTableRecord> )
            Claim.equal 3 dataTable.Columns.Count
            Claim.equal 1000L (dataTable |> DataTable.getValue 0 0)
            Claim.equal true (dataTable |> DataTable.getValue 0 1)
            Claim.equal "ABC" (dataTable |> DataTable.getValue 0 2)

        [<Fact>]
        let ``Created DataTable from list of record values - strongly typed``() =
            let recordValues = [
                {Field01 = 1000L; Field02 = true; Field03 = "ABC"}
                {Field01 = 1001L; Field02 = false; Field03 = "XYZ"}
                {Field01 = 1002L; Field02 = true; Field03 = "FGH"}
            ]

            let dataTable = recordValues |> DataTable.fromProxyValuesT
            Claim.equal 3 dataTable.Columns.Count
            Claim.equal 1000L (dataTable |> DataTable.getValue 0 0)
            Claim.equal true (dataTable |> DataTable.getValue 0 1)
            Claim.equal "ABC" (dataTable |> DataTable.getValue 0 2)


        [<Fact>]
        let ``Created record values from a DataTable - weakly  typed``() =
            let description = typeinfo<DataTableRecord>
        
            let src = 
                [
                    {Field01 = 1000L; Field02 = true; Field03 = "ABC"} :> obj
                    {Field01 = 1001L; Field02 = false; Field03 = "XYZ"} :> obj
                    {Field01 = 1002L; Field02 = true; Field03 = "FGH"} :> obj
                ] 

            let dst =  src |> DataTable.fromProxyValues (tabularproxy<DataTableRecord>)
                           |> DataTable.toProxyValuesT<obj> description
                           |> List.ofSeq
            Claim.equal src.[0] dst.[0]
            Claim.equal src.[1] dst.[1]
            Claim.equal src.[2] dst.[2]


