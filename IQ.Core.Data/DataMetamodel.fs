namespace IQ.Core.Data

open System
open System.Data
open System.Diagnostics


/// <summary>
/// Defines a metamodel that describes data models
/// </summary>
[<AutoOpen>]
module DataMetamodel = 
    /// <summary>
    /// Responsible for identifying a data object within the scope of some catalog
    /// </summary>
    type DataObjectName = DataObjectName of schemaName : string * localName : string
    
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

    /// <summary>
    /// Specifies a storage class together with the information that is required to
    /// instantiate and store values corresponding to that class
    /// </summary>
    type StorageType =
        | Unspecified
        | BitStorage
        | UInt8Storage
        | UInt16Storage
        | UInt32Storage
        | UInt64Storage
        | Int8Storage
        | Int16Storage
        | Int32Storage
        | Int64Storage
        | BinaryFixedStorage of length : int
        | BinaryVariableStorage of length : int
        | BinaryMaxStorage
        | AnsiTextFixedStorage of length : int
        | AnsiTextVariableStorage of length : int
        | AnsiTextMaxStorage
        | UnicodeTextFixedStorage of length : int
        | UnicodeTextVariableStorage of length : int
        | UnicodeTextMaxStorage
        | DateTime32Storage
        | DateTime64Storage
        | DateTimeStorage of precision : uint8
        | DateTimeOffsetStorage
        | TimeOfDayStorage
        | Float32Storage
        | Float64Storage
        | DecimalStorage of precision : uint8 * scale : uint8
        | MoneyStorage
        | GuidStorage
        | XmlStorage of schema : string
        | VariantStorage
        | CustomTableStorage of name : DataObjectName
        | CustomObjectStorage of name : DataObjectName * clrType : Type
        | CustomPrimitiveStorage of name : DataObjectName * basePrimitive : StorageType


    /// <summary>
    /// Enumerates the available means that lead to a column being automatically populated
    /// with a valid value
    /// </summary>
    type AutoValueKind =
        /// Column is not automatically populated
        | None = 0
        /// Column is automatically populated with a default value
        | Default = 1
        /// Column is automatically populated with an identity value
        | Identity = 2
        /// Column is automatically populated with a computed value
        | Computed = 3
        /// Column is automatically populated with a value from a sequence
        | Sequence = 4
    
    /// <summary>
    /// Describes a column in a table or view
    /// </summary>
    [<DebuggerDisplay("{Position} {Name,nq} {StorageType}")>]
    type ColumnDescription = {
        /// The column's name
        Name : string
        
        /// The column's position relative to the other columns
        Position : int

        /// The column's data type
        StorageType : StorageType option
        
        /// Specifies whether the column allows null
        Nullable : bool   
        
        /// Specifies the means by which the column is automatically populated, if applicable 
        AutoValue : AutoValueKind option    
    }

    /// <summary>
    /// Describes a table
    /// </summary>
    type TableDescription = {
        /// The name of the table
        Name : DataObjectName
        
        /// Specifies the  purpose of the table
        Description : string option

        /// The columns in the table
        Columns : ColumnDescription list
    }
   

[<AutoOpen>]
module DatMetamodelExtensions =
    /// <summary>
    /// Defines augmentations for the TableDescription type
    /// </summary>
    type TableDescription 
    with
        /// <summary>
        /// Finds a column identified by its name
        /// </summary>
        /// <param name="name">The name of the column</param>
        member this.Item(name) = 
            this.Columns |> List.find(fun column -> column.Name = name)

        /// <summary>
        /// Finds a column identified by its position
        /// </summary>
        /// <param name="position">The position of the column</param>
        member this.Item(position) = 
            this.Columns |> List.find(fun column -> column.Position = position)
    

