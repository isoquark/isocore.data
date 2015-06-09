﻿namespace IQ.Core.Data.Sql.Test

open IQ.Core.TestFramework

open System
open System.Data

open IQ.Core.Framework
open IQ.Core.Data

[<TestFixture>]
module DataTableTest =
    
    type private DataTableRecord = {
        Field01 : int64
        Field02 : bool
        Field03 : string
    }

    [<Test>]
    let ``Created DataTable from record metadata``() =
        let dataTable = recinfo<DataTableRecord> |> DataTable.fromRecordDescription
        Claim.equal 3 dataTable.Columns.Count
        Claim.equal "Field01" dataTable.Columns.[0].ColumnName
        Claim.equal (typeof<int64>) dataTable.Columns.[0].DataType


    [<Test>]
    let ``Created DataTable from list of record values - weakly typed``() =
        let recordValues = [
            {Field01 = 1000L; Field02 = true; Field03 = "ABC"} :> obj
            {Field01 = 1001L; Field02 = false; Field03 = "XYZ"} :> obj
            {Field01 = 1002L; Field02 = true; Field03 = "FGH"} :> obj
        ]

        let dataTable = recordValues |> DataTable.fromRecordValues
        Claim.equal 3 dataTable.Columns.Count
        Claim.equal 1000L (dataTable |> DataTable.getValue 0 0)
        Claim.equal true (dataTable |> DataTable.getValue 0 1)
        Claim.equal "ABC" (dataTable |> DataTable.getValue 0 2)

    [<Test>]
    let ``Created DataTable from list of record values - strongly typed``() =
        let recordValues = [
            {Field01 = 1000L; Field02 = true; Field03 = "ABC"}
            {Field01 = 1001L; Field02 = false; Field03 = "XYZ"}
            {Field01 = 1002L; Field02 = true; Field03 = "FGH"}
        ]

        let dataTable = recordValues |> DataTable.fromRecordValuesT
        Claim.equal 3 dataTable.Columns.Count
        Claim.equal 1000L (dataTable |> DataTable.getValue 0 0)
        Claim.equal true (dataTable |> DataTable.getValue 0 1)
        Claim.equal "ABC" (dataTable |> DataTable.getValue 0 2)


    [<Test>]
    let ``Created record values from a DataTable``() =
        let description = recinfo<DataTableRecord>
        
        let src = 
            [
                {Field01 = 1000L; Field02 = true; Field03 = "ABC"} :> obj
                {Field01 = 1001L; Field02 = false; Field03 = "XYZ"} :> obj
                {Field01 = 1002L; Field02 = true; Field03 = "FGH"} :> obj
            ] 

        let dst = src |> DataTable.fromRecordValues |> DataTable.toRecordValues description
        Claim.equal src.[0] dst.[0]
        Claim.equal src.[1] dst.[1]
        Claim.equal src.[2] dst.[2]

