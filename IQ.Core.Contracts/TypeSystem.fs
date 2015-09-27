// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework.Contracts

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
        let MaxLengthFacet = "MaxLengthFacet"

        /// Specifies that subject values must always be of a specified, fixed length
        [<Literal>]
        let FixedLengthFacet = "FixedLength"

        /// Specifies a pattern, such as a regular expression, that the subject values are required to satisfy
        [<Literal>]
        let PatternFacet = 8

        /// Specifies the focument/file format type. For example, JSON, XML, HTML, ZIP ...

        [<Literal>]
        let DocumentTypeFacet = 9
        /// Specifies whether the value/structure of the subject can be altered following creation

        [<Literal>]
        let ImmutableFacet = 10
        /// Specifies the name of a runtime type

        [<Literal>]
        let RuntimeTypeFacet = 11
    
    //A character set is a set of symbols and encodings. A collation is a set of rules for comparing characters in a character set
    //See: http://stackoverflow.com/questions/341273/what-does-character-set-and-collation-mean-exactly
    type Primitive =
        /// A whole number
        | Integer of Signed : bool * Bits : uint8
        /// A precise decimal
        | Decimal of Precision : uint8 * Scale : uint8
        /// A floating-point decimal
        | Real of Bits : uint8
        //Assumed to be unicode and of arbitrary length in the absence of applicable facets
        | Text 
        /// Specifies a date, in terms of the Gregorian calendary in the absence of applicable facets
        | Date 
        /// Specifies an instant in time
        | DateTime of Precision : uint8 * Scale : uint8
        /// Specifies the time of day
        | Time  of Precision : uint8 * Scale : uint8
        /// A non-negative interval of time measured in nanoseconds
        | Duration of Bits : uint8
        /// True or False, 0 or 1, etc.
        | Boolean
        //A single byte
        | Byte
        /// An ordered sequence of bytes, assumed to be of arbitrary length in the absence of constraining facets
        | ByteArray
        /// A specialized primitive
        | CustomPrimitive of Name : string * Base : Primitive * Facets : Facet list

    /// <summary>
    /// Represents a reference/instantiation of a primitive type within some context, e.g.,
    /// a an entity definition, a database column, etc.
    /// </summary>
    type PrimitiveReference = PrimitiveReference of DataType : Primitive * Facets : Facet list
      
    /// <summary>
    /// Represents an intrinsic notion of a table column
    /// </summary>
    type TableColumn = TableColumn of Name : string * Position : int * DataType : PrimitiveReference

    /// <summary>
    /// Defines a data type
    /// </summary>
    type DataType =
        /// A primitive data type
        | PrimitiveType of Primitive 
        /// A tabular dataset
        | TableType of Name : string * Columns : TableColumn list
        /// A fixed-length ordered collection of elements whose members can be identified/accesed by their 
        /// relative position in the collection
        | ArrayType of ElementType : DataType * Length : int
        /// A variable-length ordered collection of elements whose members can be identified/accesed by their 
        /// relative position in the collection
        | ListType of DataType
        /// A collection of elements whose membes can be identified/accessed by a key
        | DictionaryType of KeyType : Primitive * ValueType : DataType
        /// A collection of elements
        | SetType of DataType
        /// A context-sensitive "object" value, whatever "object" means in the concrete system
        /// and the value of the name field can identify the type in that context
        | ObjectType of name : string 

    /// <summary>
    /// Represents a reference to a data type
    /// </summary>
    type DataTypeReference = DataTypeReference of DataType * Facets : Facet list

    type EntityProperty = EntityProperty of Name : string * Type : DataTypeReference

    type DataEntityType = DataEntityType of Name : string  * Properties : EntityProperty list


