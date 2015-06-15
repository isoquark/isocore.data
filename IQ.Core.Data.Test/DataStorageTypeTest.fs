namespace IQ.Core.Data.Test

open System
open System.Diagnostics

open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Data


[<TestContainer>]
module ``DataStorageType Test`` =

    [<Test>]
    let ``Parsed semantic representations of DataStorageType``() =
        "Bit" |> DataStorageType.parse  |> Option.get |> Claim.equal BitStorage
        "UInt8" |> DataStorageType.parse  |> Option.get |> Claim.equal UInt8Storage
        "BinaryVariable(350)" |>  DataStorageType.parse |> Option.get |> Claim.equal (BinaryVariableStorage(350))
        "BinaryFixed(120)" |>  DataStorageType.parse |> Option.get |> Claim.equal (BinaryFixedStorage(120))

    [<Test>]
    let ``Rendered semantic representations of DataStorageType``() =       
        122 |> AnsiTextFixedStorage |> DataStorageType.toSemanticString |> Claim.equal "AnsiTextFixed(122)"
        122 |> AnsiTextVariableStorage |> DataStorageType.toSemanticString |> Claim.equal "AnsiTextVariable(122)"
        AnsiTextMaxStorage |> DataStorageType.toSemanticString |> Claim.equal "AnsiTextMax"
        120 |> BinaryFixedStorage |> DataStorageType.toSemanticString |> Claim.equal "BinaryFixed(120)"
        350 |> BinaryVariableStorage |> DataStorageType.toSemanticString |> Claim.equal "BinaryVariable(350)"
        BinaryMaxStorage |> DataStorageType.toSemanticString |> Claim.equal "BinaryMax"
    
    let private verifyAttribute storageTypeExpect (attribute : StorageTypeAttribute) =
        attribute |> DataStorageType.fromAttribute |> Claim.equal storageTypeExpect


    [<Test>]
    let ``Determined StorageType values from data attributes``() =
        StorageTypeAttribute(StorageKind.AnsiTextFixed, 150) |> verifyAttribute (AnsiTextFixedStorage(150))
        StorageTypeAttribute(StorageKind.AnsiTextMax) |> verifyAttribute (AnsiTextMaxStorage)
        StorageTypeAttribute(StorageKind.AnsiTextVariable, 150) |> verifyAttribute (AnsiTextVariableStorage(150))
        StorageTypeAttribute(StorageKind.BinaryFixed, 150) |> verifyAttribute (BinaryFixedStorage(150))
        StorageTypeAttribute(StorageKind.BinaryMax) |> verifyAttribute (BinaryMaxStorage)
        StorageTypeAttribute(StorageKind.BinaryVariable, 150) |> verifyAttribute (BinaryVariableStorage(150))
        StorageTypeAttribute(StorageKind.Bit) |> verifyAttribute BitStorage
        StorageTypeAttribute(StorageKind.CustomObject, typeof<int>, "X", "Y") |> verifyAttribute (CustomObjectStorage(DataObjectName("X", "Y"), typeof<int>))        
        StorageTypeAttribute(StorageKind.CustomPrimitive, "X", "Y") |> verifyAttribute (CustomPrimitiveStorage(DataObjectName("X","Y")))
        StorageTypeAttribute(StorageKind.CustomTable, "X", "Y") |> verifyAttribute (CustomTableStorage(DataObjectName("X","Y")))
        StorageTypeAttribute(StorageKind.Date) |> verifyAttribute DateStorage
        StorageTypeAttribute(StorageKind.DateTime, 3uy) |> verifyAttribute (DateTimeStorage(3uy))
        StorageTypeAttribute(StorageKind.DateTime32) |> verifyAttribute (DateTime32Storage)
        StorageTypeAttribute(StorageKind.DateTime64) |> verifyAttribute (DateTime64Storage)
        StorageTypeAttribute(StorageKind.DateTimeOffset) |> verifyAttribute (DateTimeOffsetStorage)
        StorageTypeAttribute(StorageKind.Decimal,12uy,4uy) |> verifyAttribute (DecimalStorage(12uy,4uy))
        StorageTypeAttribute(StorageKind.Float32) |> verifyAttribute (Float32Storage)
        StorageTypeAttribute(StorageKind.Float64) |> verifyAttribute (Float64Storage)
        StorageTypeAttribute(StorageKind.Guid) |> verifyAttribute (GuidStorage)
        StorageTypeAttribute(StorageKind.Int8) |> verifyAttribute (Int8Storage)
        StorageTypeAttribute(StorageKind.Int16) |> verifyAttribute (Int16Storage)
        StorageTypeAttribute(StorageKind.Int32) |> verifyAttribute (Int32Storage)
        StorageTypeAttribute(StorageKind.Int64) |> verifyAttribute (Int64Storage)        
        StorageTypeAttribute(StorageKind.Money) |> verifyAttribute MoneyStorage
        StorageTypeAttribute(StorageKind.TimeOfDay) |> verifyAttribute TimeOfDayStorage
        StorageTypeAttribute(StorageKind.UInt8) |> verifyAttribute (UInt8Storage)
        StorageTypeAttribute(StorageKind.UInt16) |> verifyAttribute (UInt16Storage)
        StorageTypeAttribute(StorageKind.UInt32) |> verifyAttribute (UInt32Storage)
        StorageTypeAttribute(StorageKind.UInt64) |> verifyAttribute (UInt64Storage)
        StorageTypeAttribute(StorageKind.UnicodeTextFixed, 150) |> verifyAttribute (UnicodeTextFixedStorage(150))
        StorageTypeAttribute(StorageKind.UnicodeTextMax) |> verifyAttribute (UnicodeTextMaxStorage)
        StorageTypeAttribute(StorageKind.UnicodeTextVariable, 150) |> verifyAttribute (UnicodeTextVariableStorage(150))
        StorageTypeAttribute(StorageKind.Variant) |> verifyAttribute VariantStorage
        StorageTypeAttribute(StorageKind.Xml) |> verifyAttribute ( XmlStorage(""))

        ()