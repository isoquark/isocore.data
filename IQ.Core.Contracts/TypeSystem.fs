// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework.Contracts

open System

/// <summary>
/// Defines common vocabulary for conforming and converting between different type sytems
/// such as: CLR/.Net, SQL Server, Power Query, Python, etc.
/// </summary>
module UniversalTypeSystem =
    /// <summary>
    /// Represents a characteristic applied to a data type
    /// </summary>
    type Facet = Facet of Name : string * Value : string
    with
        static member Create (name, value: 'T) = Facet(name, value.ToString())
             
    /// <summary>
    /// Codifies a set of common facets
    /// </summary>
    module StandardFacetNames =        
        ///Specifes a code page for character data
        ///See: https://msdn.microsoft.com/en-us/library/windows/desktop/dd317756(v=vs.85).aspx
        [<Literal>]
        let CodePageFacet = "CodePage"

        ///Specifes a collation for character data
        [<Literal>]
        let CollationFacet = "Collation"

        ///Specifies an (inclusive) minimum value (for a data type for which a comparison operation is defined)
        [<Literal>]
        let MinValueFacet = "MinValue"

        ///Specifies an (inclusive) maximum value (for a data type for which a comparison operation is defined)
        [<Literal>]
        let MaxValueFacet = "MaxValue"

        /// Specifies that the length of subject values can be no less than than a specified value
        [<Literal>]
        let MinLengthFacet = "MinLength"

        /// Specifies that the length of subject values can be no greater than a specified value
        [<Literal>]
        let MaxLengthFacet = "MaxLength"

        /// Specifies that subject values must always be of a specified, fixed length
        [<Literal>]
        let FixedLengthFacet = "FixedLength"

        /// Specifies a pattern, such as a regular expression, that the subject values are required to satisfy
        [<Literal>]
        let PatternFacet = "Pattern"

        /// Specifies the focument/file format type. For example, JSON, XML, HTML, ZIP ...
        [<Literal>]
        let DocumentTypeFacet = "DocumentType"
        
        /// Specifies whether the value/structure of the subject can be altered following creation
        [<Literal>]
        let ImmutableFacet = "Immutable"
        
        /// Specifies the name of a runtime type
        [<Literal>]
        let RuntimeTypeFacet = "RuntimeType"

        /// Specifies the order of the element relevant to some context
        [<Literal>]
        let Position = "Position"
    
    type IntegerValue =
        /// 14 : int(u, 8)
        /// 14 : uint8
        /// 14uy
        | UInt8Value of uint8
        | Int8Value of int8
        | UInt16Value of uint16
        | Int16Value of int16
        | UInt32Value of uint32
        | Int32Value of int32
        | UInt64Value of uint64
        | Int64Value of int64

    type RealValue =
        | Float32Value of float32
        | Float64Value of float

    //A character set is a set of symbols and encodings. A collation is a set of rules for comparing characters in a character set
    //See: http://stackoverflow.com/questions/341273/what-does-character-set-and-collation-mean-exactly
    type PrimitiveType =
        /// A whole number
        /// int(s|u, n)[f1 = v1; ...; fm = vm]
        /// Aliases for common types
        /// int16 := int(s, 16)
        /// uint16 := int(u, 16)
        /// ...
        | Integer of Signed : bool * Bits : uint8
        /// A precise decimal
        /// dec(p,s)
        | Decimal of Precision : uint8 * Scale : uint8
        /// A floating-point decimal
        /// real(n)
        | Real of Bits : uint8
        //Assumed to be unicode and of arbitrary length in the absence of applicable facets
        /// text[MinLength=50; MaxLength=100]
        | Text 
        /// Specifies a date, in terms of the Gregorian calendary in the absence of applicable facets
        /// date
        | Date 
        /// Specifies an instant in time
        /// datetime(p,s)
        | DateTime of Precision : uint8 * Scale : uint8
        /// Specifies the time of day
        /// time(p,s)
        | Time  of Precision : uint8 * Scale : uint8
        /// A non-negative interval of time measured in nanoseconds
        /// duration(n)
        | Duration of Bits : uint8
        /// True or False, 0 or 1, etc.
        /// bool
        | Boolean
        //A single byte
        /// byte
        | Byte
        /// An ordered sequence of bytes, assumed to be of arbitrary length in the absence of constraining facets
        /// blob
        | Blob
        /// A specialized primitive
        /// name( int(s,32), [f1=v1; ...; fn=vn])
        | CustomPrimitive of Name : string * Base : PrimitiveType * Facets : Facet list

    type PrimitiveValue =
        | IntegerValue of IntegerValue
        | DecimalValue of decimal
        | RealValue of RealValue
        | TextValue of string
        | DateValue of DateTime
        | DateTimeValue of DateTime
        | TimeValue of float
        | DurationValue of IntegerValue
        | BooleanValue of bool
        | ByteValue of uint8
        | ByteArray of uint8[]
        | CustomValue of uint8[]

    /// <summary>
    /// Represents a reference/instantiation of a primitive type within some context, e.g.,
    /// a an entity definition, a database column, etc.
    /// </summary>
    type PrimitiveReference = PrimitiveReference of DataType : PrimitiveType * Facets : Facet list
      
    /// <summary>
    /// Represents an intrinsic notion of a table column
    /// column(0, Col01, int(s, 32)[?])
    /// column(1, Col02, dec(19, 4)[?])
    /// </summary>
    type TableColumn = TableColumn of Name : string * Position : int * DataType : PrimitiveReference

    

    /// <summary>
    /// Defines a data type
    /// </summary>
    type DataType =
        /// A primitive data type
        | PrimitiveType of PrimitiveType 
        /// A tabular dataset
        /// table(name, column(0, Col01, int(s, 32)[?]),  column(1, Col02, dec(19, 4)[?]), ...)        
        | TableType of Name : string * Columns : TableColumn list
        /// A collection of elements        
        | CollectionType of CollectionType
        /// A context-sensitive "object" value, whatever "object" means in the concrete system
        /// and the value of the name field can identify the type in that context
        /// object(name)
        | ObjectType of name : string 
    and CollectionType =
        /// A fixed-length ordered collection of elements 
        /// array<T>(50)
        | ArrayType of ElementType : DataType * Length : int
        /// A variable-length ordered collection of elements whose members can be identified/accesed by their 
        /// relative position in the collection
        /// list<T>
        | ListType of DataType
        /// A collection of elements whose membes can be identified/accessed by a key
        /// dict<T,K>
        | DictionaryType of KeyType : PrimitiveType * ValueType : DataType
        /// A collection of elements
        /// set<T>
        | SetType of DataType


    /// <summary>
    /// Represents a reference to a data type
    /// </summary>
    type DataTypeReference = DataTypeReference of DataType * Facets : Facet list

    type EntityProperty = EntityProperty of Name : string * Type : DataTypeReference

    type DataEntityType = DataEntityType of Name : string  * Properties : EntityProperty list


