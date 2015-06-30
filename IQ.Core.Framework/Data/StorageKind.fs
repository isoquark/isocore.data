namespace IQ.Core.Data

open System
open System.Collections.Generic

open FSharp.Data

open IQ.Core.Framework

[<AutoOpen>]
module StorageKindVocabulary =
    /// <summary>
    /// Specifies the available storage classes
    /// </summary>
    /// <remarks>
    /// Note that the storage class is not sufficient to characterize the storage type and
    /// additional information, such as length or data object name is needed to store/instantiate
    /// a corresponding value
    /// </remarks>
    type StorageKind =
        | Unspecified = 0uy
        | Bit = 10uy //bit
        | UInt8 = 20uy //tinyint
        | UInt16 = 21uy //no direct map, use int
        | UInt32 = 22uy // no direct map, use bigint
        | UInt64 = 23uy // no direct map, use varbinary(8)
        | Int8 = 30uy //no direct map, use smallint
        | Int16 = 31uy //smallint
        | Int32 = 32uy //int
        | Int64 = 33uy //bigint
        | BinaryFixed = 40uy //binary 
        | BinaryVariable = 41uy //varbinary
        | BinaryMax = 42uy
        | AnsiTextFixed = 50uy //char
        | AnsiTextVariable = 51uy //varchar
        | AnsiTextMax = 52uy
        | UnicodeTextFixed = 53uy //nchar
        | UnicodeTextVariable = 54uy //nvarchar
        | UnicodeTextMax = 55uy
        | DateTime32 = 60uy //corresponds to smalldatetime
        | DateTime64 = 61uy //corresponds to datetime
        | DateTime = 62uy //corresponds to datetime2
        | DateTimeOffset = 63uy
        | TimeOfDay = 64uy //corresponds to time
        | Date = 65uy //corresponds to date
        | Timespan = 66uy //no direct map, use bigint to store number of ticks
        | Float32 = 70uy //corresponds to real
        | Float64 = 71uy //corresponds to float
        | Decimal = 80uy
        | Money = 81uy
        | Guid = 90uy //corresponds to uniqueidentifier
        | Xml = 100uy
        | Variant = 110uy //corresponds to sql_variant
        | CustomTable = 150uy //a non-intrinsic table data type
        | CustomObject = 151uy //a non-intrinsic CLR type
        | CustomPrimitive = 152uy //a non-intrinsic primitive based on an intrinsic primitive
        | Geography = 160uy
        | Geometry = 161uy
        | Hierarchy = 162uy
        | TypedDocument = 180uy


module StorageKind =
        [<Literal>]
        let private DefaultStorageKindAspectsResource = "Data/Resources/DefaultStorageKindAspects.csv"                
        type private DefaultStorageKindAspects = CsvProvider<DefaultStorageKindAspectsResource, Separators="|", PreferOptionals=true>
        
        type private StorageKindAspects = | StorageKindAspects of length : int option * precision : uint8 option * scale : uint8 option
        with
            member this.Length = match this with StorageKindAspects(length=x) -> x |> Option.get
            member this.Precision = match this with StorageKindAspects(precision=x) -> x |> Option.get
            member this.Scale = match this with StorageKindAspects(scale=x) -> x |> Option.get

        let private defaults : IDictionary<StorageKind, StorageKindAspects> = 
            [for row in (DefaultStorageKindAspectsResource |> DefaultStorageKindAspects.Load).Cache().Rows ->
                (StorageKind.Parse row.StorageKindName, StorageKindAspects(row.Length, row.Precision |> Convert.ToUInt8Option , row.Scale |> Convert.ToUInt8Option))
            ] |> dict        
        
        /// <summary>
        /// Gets the storage kind's default length
        /// </summary>
        /// <param name="kind">The kind of storage</param>
        let getDefaultLength kind =
            defaults.[kind].Length 

        /// <summary>
        /// Gets the storage kind's default precision
        /// </summary>
        /// <param name="kind">The kind of storage</param>
        let getDefaultPrecision kind =
            defaults.[kind].Precision

        /// <summary>
        /// Gets the storage kind's default scale
        /// </summary>
        /// <param name="kind">The kind of storage</param>
        let getDefaultScale kind =
            defaults.[kind].Scale

[<AutoOpen>]
module StorageKindExtensions =
    type StorageKind
    with
        member this.DefaultLength = this |> StorageKind.getDefaultLength
        member this.DefaultPrecision = this |> StorageKind.getDefaultPrecision
        member this.DefaultScale = this |> StorageKind.getDefaultScale