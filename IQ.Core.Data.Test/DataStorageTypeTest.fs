// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Test

open System
open System.Diagnostics

open IQ.Core.Data
open IQ.Core.Framework.Test


module DataType =
//    let private verifyAttribute storageTypeExpect (attribute : DataTypeAttribute) =
//        attribute.DataType |> Claim.equal storageTypeExpect
    
    type Tests(ctx, log) =
        inherit ProjectTestContainer(ctx,log)
        [<Fact>]
        let ``Parsed semantic representations of DataStorageType``() =
            "Bit" |> DataType.parse  |> Option.get |> Claim.equal BitDataType
            "UInt8" |> DataType.parse  |> Option.get |> Claim.equal UInt8DataType
            "BinaryVariable(350)" |>  DataType.parse |> Option.get |> Claim.equal (BinaryVariableDataType(350))
            "BinaryFixed(120)" |>  DataType.parse |> Option.get |> Claim.equal (BinaryFixedDataType(120))

        [<Fact>]
        let ``Rendered semantic representations of DataStorageType``() =       
            122 |> AnsiTextFixedDataType |> DataType.toSemanticString |> Claim.equal "AnsiTextFixed(122)"
            122 |> AnsiTextVariableDataType |> DataType.toSemanticString |> Claim.equal "AnsiTextVariable(122)"
            AnsiTextMaxDataType |> DataType.toSemanticString |> Claim.equal "AnsiTextMax"
            120 |> BinaryFixedDataType |> DataType.toSemanticString |> Claim.equal "BinaryFixed(120)"
            350 |> BinaryVariableDataType |> DataType.toSemanticString |> Claim.equal "BinaryVariable(350)"
            BinaryMaxDataType |> DataType.toSemanticString |> Claim.equal "BinaryMax"
    
//        [<Fact>]
//        let ``Determined StorageType values from data attributes``() =
//            DataTypeAttribute(DataKind.AnsiTextFixed, 150) |> verifyAttribute (AnsiTextFixedDataType(150))
//            DataTypeAttribute(DataKind.AnsiTextMax) |> verifyAttribute (AnsiTextMaxDataType)
//            DataTypeAttribute(DataKind.AnsiTextVariable, 150) |> verifyAttribute (AnsiTextVariableDataType(150))
//            DataTypeAttribute(DataKind.BinaryFixed, 150) |> verifyAttribute (BinaryFixedDataType(150))
//            DataTypeAttribute(DataKind.BinaryMax) |> verifyAttribute (BinaryMaxDataType)
//            DataTypeAttribute(DataKind.BinaryVariable, 150) |> verifyAttribute (BinaryVariableDataType(150))
//            DataTypeAttribute(DataKind.Bit) |> verifyAttribute BitDataType
//            DataTypeAttribute(DataKind.CustomObject, typeof<int>, "X", "Y") |> verifyAttribute (ObjectDataType(DataObjectName("X", "Y"), typeof<int>.FullName))        
//            DataTypeAttribute(DataKind.CustomPrimitive, "X", "Y") |> verifyAttribute (CustomPrimitiveDataType(DataObjectName("X","Y"), Int32DataType))
//            DataTypeAttribute(DataKind.CustomTable, "X", "Y") |> verifyAttribute (TableDataType(DataObjectName("X","Y")))
//            DataTypeAttribute(DataKind.Date) |> verifyAttribute DateDataType
//            DataTypeAttribute(DataKind.DateTime) |> verifyAttribute (DateTimeDataType(27uy,7uy))
//            DataTypeAttribute(DataKind.DateTimeOffset) |> verifyAttribute (DateTimeOffsetDataType)
//            DataTypeAttribute(DataKind.Decimal,12uy,4uy) |> verifyAttribute (DecimalDataType(12uy,4uy))
//            DataTypeAttribute(DataKind.Float32) |> verifyAttribute (Float32DataType)
//            DataTypeAttribute(DataKind.Float64) |> verifyAttribute (Float64DataType)
//            DataTypeAttribute(DataKind.Guid) |> verifyAttribute (GuidDataType)
//            DataTypeAttribute(DataKind.Int8) |> verifyAttribute (Int8DataType)
//            DataTypeAttribute(DataKind.Int16) |> verifyAttribute (Int16DataType)
//            DataTypeAttribute(DataKind.Int32) |> verifyAttribute (Int32DataType)
//            DataTypeAttribute(DataKind.Int64) |> verifyAttribute (Int64DataType)        
//            DataTypeAttribute(DataKind.Money) |> verifyAttribute (MoneyDataType(19uy,4uy))
//            DataTypeAttribute(DataKind.TimeOfDay) |> verifyAttribute (TimeOfDayDataType(16uy,7uy))
//            DataTypeAttribute(DataKind.UInt8) |> verifyAttribute (UInt8DataType)
//            DataTypeAttribute(DataKind.UInt16) |> verifyAttribute (UInt16DataType)
//            DataTypeAttribute(DataKind.UInt32) |> verifyAttribute (UInt32DataType)
//            DataTypeAttribute(DataKind.UInt64) |> verifyAttribute (UInt64DataType)
//            DataTypeAttribute(DataKind.UnicodeTextFixed, 150) |> verifyAttribute (UnicodeTextFixedDataType(150))
//            DataTypeAttribute(DataKind.UnicodeTextMax) |> verifyAttribute (UnicodeTextMaxDataType)
//            DataTypeAttribute(DataKind.UnicodeTextVariable, 150) |> verifyAttribute (UnicodeTextVariableDataType(150))
//            DataTypeAttribute(DataKind.Flexible) |> verifyAttribute VariantDataType
//            DataTypeAttribute(DataKind.Xml) |> verifyAttribute ( XmlDataType(""))


