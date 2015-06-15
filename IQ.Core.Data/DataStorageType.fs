namespace IQ.Core.Data

open System
open System.Data
open System.Diagnostics
open System.Text
open System.Reflection
open System.Text.RegularExpressions

open IQ.Core.Framework

/// <summary>
/// Defines the Data Storage Type domain vocabulary
[<AutoOpen>]
module DataStorageTypeVocabulary =

    /// <summary>
    /// Defines the literals that specify the semantic names for the StorageType cases
    /// </summary>
    module StorageTypeNames =
        [<Literal>]
        let BitStorageName = "Bit"
        [<Literal>]
        let UInt8StorageName = "UInt8"
        [<Literal>]
        let UInt16StorageName = "UInt16"
        [<Literal>]
        let UInt32StorageName = "UInt32"
        [<Literal>]
        let UInt64StorageName = "UInt64"
        [<Literal>]
        let Int8StorageName = "Int8"
        [<Literal>]
        let Int16StorageName = "Int16"
        [<Literal>]
        let Int32StorageName = "Int32"
        [<Literal>]
        let Int64StorageName = "Int64"
        [<Literal>]
        let BinaryFixedStorageName = "BinaryFixed"
        [<Literal>]
        let BinaryVariableStorageName = "BinaryVariable"
        [<Literal>]
        let BinaryMaxStorageName = "BinaryMax"
        [<Literal>]
        let AnsiTextFixedStorageName = "AnsiTextFixed"
        [<Literal>]
        let AnsiTextVariableStorageName = "AnsiTextVariable"
        [<Literal>]
        let AnsiTextMaxStorageName = "AnsiTextMax"
        [<Literal>]
        let UnicodeTextFixedStorageName = "UnicodeTextFixed"
        [<Literal>]
        let UnicodeTextVariableStorageName = "UnicodeTextVariable"
        [<Literal>]
        let UnicodeTextMaxStorageName = "UnicodeTextMax"
        [<Literal>]
        let DateTime32StorageName = "DateTime32"
        [<Literal>]
        let DateTime64StorageName = "DateTime64"
        [<Literal>]
        let DateTimeStorageName = "DateTime"
        [<Literal>]
        let DateTimeOffsetStorageName = "DateTimeOffset"
        [<Literal>]
        let TimeOfDayStorageName = "TimeOfDay"
        [<Literal>]
        let DateStorageName = "Date"
        [<Literal>]
        let Float32StorageName = "Float32"
        [<Literal>]
        let Float64StorageName = "Float64"
        [<Literal>]
        let DecimalStorageName = "Decimal"
        [<Literal>]
        let MoneyStorageName = "Money"
        [<Literal>]
        let GuidStorageName = "Guid"
        [<Literal>]
        let XmlStorageName = "Xml"
        [<Literal>]
        let VariantStorageName = "Variant"
        [<Literal>]
        let CustomTableStorageName = "CustomTable"
        [<Literal>]
        let CustomObjectStorageName = "CustomObject"
        [<Literal>]
        let CustomPrimitiveStorageName = "CustomPrimitive"


    open StorageTypeNames
    /// <summary>
    /// Specifies a storage class together with the information that is required to
    /// instantiate and store values corresponding to that class
    /// </summary>
    [<DebuggerDisplay(DebuggerDisplayDefault)>]
    type StorageType =
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
        | DateStorage
        | Float32Storage
        | Float64Storage
        | DecimalStorage of precision : uint8 * scale : uint8
        | MoneyStorage
        | GuidStorage
        | XmlStorage of schema : string
        | VariantStorage
        | CustomTableStorage of name : DataObjectName
        | CustomObjectStorage of name : DataObjectName * clrType : Type
        | CustomPrimitiveStorage of name : DataObjectName 
    with        
        /// <summary>
        /// Renders a faithful representation of an instance as text
        /// </summary>
        member this.ToSemanticString() =
            match this with
            | BitStorage -> BitStorageName
            | UInt8Storage -> UInt8StorageName
            | UInt16Storage -> UInt16StorageName
            | UInt32Storage -> UInt32StorageName
            | UInt64Storage -> UInt64StorageName
            | Int8Storage -> Int8StorageName
            | Int16Storage -> Int16StorageName
            | Int32Storage -> Int32StorageName
            | Int64Storage -> Int64StorageName            
            
            | BinaryFixedStorage(length) -> length |> sprintf "%s(%i)" BinaryFixedStorageName
            | BinaryVariableStorage(length) -> length |> sprintf "%s(%i)" BinaryVariableStorageName
            | BinaryMaxStorage -> BinaryMaxStorageName
            
            | AnsiTextFixedStorage(length) -> length |> sprintf "%s(%i)" AnsiTextFixedStorageName
            | AnsiTextVariableStorage(length) -> length |> sprintf "%s(%i)" AnsiTextVariableStorageName
            | AnsiTextMaxStorage -> AnsiTextMaxStorageName
            
            | UnicodeTextFixedStorage(length) -> length |> sprintf "%s(%i)" UnicodeTextFixedStorageName
            | UnicodeTextVariableStorage(length) -> length |> sprintf "%s(%i)" UnicodeTextVariableStorageName
            | UnicodeTextMaxStorage -> UnicodeTextMaxStorageName
            
            | DateTime32Storage -> DateTime32StorageName
            | DateTime64Storage -> DateTime64StorageName
            | DateTimeStorage(precision)-> precision |> sprintf "%s(%i)" DateTimeStorageName
            | DateTimeOffsetStorage -> DateTimeOffsetStorageName
            | TimeOfDayStorage -> TimeOfDayStorageName
            | DateStorage -> DateStorageName
            
            | Float32Storage -> Float32StorageName
            | Float64Storage -> Float64StorageName
            | DecimalStorage(precision,scale) -> sprintf "%s(%i,%i)" DecimalStorageName precision scale
            | MoneyStorage -> MoneyStorageName
            | GuidStorage -> GuidStorageName
            | XmlStorage(schema) -> schema |> sprintf "%s(%s)" XmlStorageName
            | VariantStorage -> VariantStorageName
            | CustomTableStorage(name) -> name |> sprintf "%s%O" CustomTableStorageName
            | CustomObjectStorage(name,t) -> sprintf "%s%O:%s" CustomObjectStorageName name t.AssemblyQualifiedName
            | CustomPrimitiveStorage(name) -> sprintf "%s%O" CustomPrimitiveStorageName name 

        /// <summary>
        /// Renders a representation of an instance as text
        /// </summary>
        override this.ToString() =
            this.ToSemanticString()
    

             
        

open StorageTypeNames

module DataStorageType =
        
        /// <summary>
        /// Renders the StorageType as a semantic string
        /// </summary>
        /// <param name="storageType">The storage type</param>
        let toSemanticString (storageType : StorageType) =
            storageType.ToSemanticString()            
        
        /// <summary>
        /// Infers the storage type from a supplied attribute
        /// </summary>
        /// <param name="attrib">The attribute that describes the type of storage</param>
        let fromAttribute (attrib : StorageTypeAttribute)=
            match attrib.StorageKind with
            | StorageKind.Bit ->BitStorage
            | StorageKind.UInt8 -> UInt8Storage
            | StorageKind.UInt16 -> UInt16Storage
            | StorageKind.UInt32 -> UInt32Storage
            | StorageKind.UInt64 -> UInt64Storage
            | StorageKind.Int8 -> Int8Storage
            | StorageKind.Int16 -> Int16Storage
            | StorageKind.Int32 -> Int32Storage
            | StorageKind.Int64 -> Int64Storage
            | StorageKind.Float32 -> Float32Storage
            | StorageKind.Float64 -> Float64Storage
            | StorageKind.Money -> MoneyStorage
            | StorageKind.Guid -> GuidStorage
            | StorageKind.AnsiTextMax -> AnsiTextMaxStorage
            | StorageKind.DateTime32 -> DateTime32Storage
            | StorageKind.DateTime64 -> DateTime64Storage
            | StorageKind.DateTimeOffset -> DateTimeOffsetStorage
            | StorageKind.TimeOfDay -> TimeOfDayStorage
            | StorageKind.Variant -> VariantStorage
            | StorageKind.UnicodeTextMax -> UnicodeTextMaxStorage
            | StorageKind.BinaryFixed -> BinaryFixedStorage( defaultArg attrib.Length 250)
            | StorageKind.BinaryVariable -> BinaryVariableStorage (defaultArg attrib.Length 250)
            | StorageKind.BinaryMax -> BinaryMaxStorage
            | StorageKind.AnsiTextFixed -> AnsiTextFixedStorage(defaultArg attrib.Length 250)
            | StorageKind.AnsiTextVariable -> AnsiTextVariableStorage(defaultArg attrib.Length 250)
            | StorageKind.UnicodeTextFixed -> UnicodeTextFixedStorage(defaultArg attrib.Length 250)
            | StorageKind.UnicodeTextVariable -> UnicodeTextVariableStorage(defaultArg attrib.Length 250)
            | StorageKind.DateTime -> DateTimeStorage(defaultArg attrib.Precision 7uy)  
            | StorageKind.Date -> DateStorage
            | StorageKind.Decimal -> DecimalStorage(defaultArg attrib.Precision 19uy, defaultArg attrib.Scale 4uy)
            | StorageKind.Xml -> XmlStorage("")
            | StorageKind.CustomTable -> CustomTableStorage(attrib.CustomTypeName |> Option.get)
            | StorageKind.CustomPrimitive -> CustomPrimitiveStorage(attrib.CustomTypeName |> Option.get)
            | StorageKind.CustomObject | StorageKind.Geography | StorageKind.Geometry | StorageKind.Hierarchy ->          
                CustomObjectStorage(attrib.CustomTypeName |> Option.get, attrib.ClrType |> Option.get)
            | _ ->
                NotSupportedException(sprintf "The storage kind %A is not recognized" attrib.StorageKind) |> raise
            

        /// <summary>
        /// Parses the semantic representation of a StorageType
        /// </summary>
        /// <param name="text">The semantic representation</param>        
        let parse text =        
            //TODO: Investigate using FParsec for this
            let pattern4() =
                let parameters = ["StorageName"; "SchemaName"; "LocalName"]
                let expression = @"(?<StorageName>[a-zA-z]*)\((?<SchemaName>[^,]*),(?<LocalName>[^\)]*)\)"
                match text |> Txt.tryMatchGroups parameters expression  with
                | Some(groups) ->
                    match groups?StorageName with
                    | CustomTableStorageName -> CustomTableStorage(DataObjectName(groups?SchemaName, groups?LocalName)) |> Some
                    | _ ->
                        None
                | None ->
                    None                   
                        
            let pattern3() =
                //This obviously won't validate a uri, but it's good enough for now
                let parameters = ["StorageName"; "p"; "s"]
                let expression = @"(?<StorageName>[a-zA-z]*)\((?<uri>(.)*)\)"
                match text |> Txt.tryMatchGroups parameters expression  with
                | Some(groups) ->
                    match groups?StorageName with
                    | XmlStorageName -> XmlStorage(groups?uri) |> Some
                    | _ ->
                        None
                | None ->
                    None                   
                
            
            let pattern2() =
                let parameters = ["StorageName"; "p"; "s"]
                let expression = @"(?<StorageName>[a-zA-z]*)\((?<p>[0-9]*),(?<s>[0-9]*)\)"
                match text |> Txt.tryMatchGroups  parameters expression with
                | Some(groups) ->
                    let p = Byte.Parse(groups?p)
                    let s = Byte.Parse(groups?s)
                    match groups?StorageName with
                    | DecimalStorageName -> DecimalStorage(p,s) |> Some
                    | _ ->
                        None
                | None ->
                    pattern3()
            
            let pattern1() =
                let parameters = ["StorageName"; "n"]
                let expression = @"(?<StorageName>[a-zA-z]*)\((?<n>[0-9]*)\)" 
                match text |> Txt.tryMatchGroups parameters expression with
                | Some(groups) ->
                    let n = Int32.Parse(groups?n)
                    match groups?StorageName with
                        | BinaryFixedStorageName -> BinaryFixedStorage(n) |> Some
                        | BinaryVariableStorageName -> BinaryVariableStorage(n) |> Some
                        | AnsiTextFixedStorageName -> AnsiTextFixedStorage(n) |> Some      
                        | AnsiTextVariableStorageName -> AnsiTextVariableStorage(n) |> Some               
                        | UnicodeTextFixedStorageName -> UnicodeTextFixedStorage(n) |> Some                
                        | UnicodeTextVariableStorageName -> UnicodeTextVariableStorage(n) |> Some  
                        | DateTimeStorageName -> DateTimeStorage(uint8(n)) |> Some
                        | _ -> None
                | None -> pattern2()

            let pattern0() =
                match text with
                | BitStorageName -> BitStorage |> Some
                | UInt8StorageName -> UInt8Storage |> Some
                | UInt16StorageName -> UInt16Storage |> Some
                | UInt32StorageName -> UInt32Storage |> Some
                | UInt64StorageName -> UInt64Storage |> Some
                | Int8StorageName -> Int8Storage |> Some
                | Int16StorageName -> Int16Storage |> Some
                | Int32StorageName -> Int32Storage |> Some
                | Int64StorageName -> Int64Storage |> Some
                | BinaryMaxStorageName -> BinaryMaxStorage |> Some
                | AnsiTextMaxStorageName -> AnsiTextMaxStorage |> Some
                | UnicodeTextMaxStorageName -> UnicodeTextMaxStorage |> Some
                | DateTime32StorageName -> DateTime32Storage |> Some
                | DateTime64StorageName -> DateTime64Storage |> Some
                | DateTimeOffsetStorageName -> DateTimeOffsetStorage |> Some
                | DateStorageName -> DateStorage |> Some
                | TimeOfDayStorageName -> TimeOfDayStorage |> Some
                | Float32StorageName -> Float32Storage |> Some
                | Float64StorageName-> Float64Storage |> Some
                | MoneyStorageName -> MoneyStorage |> Some
                | GuidStorageName -> GuidStorage |> Some
                | VariantStorageName -> VariantStorage |> Some
                | _ -> pattern1()
        
            pattern0()

            //Txt.matchRegexGroups 
            

        
