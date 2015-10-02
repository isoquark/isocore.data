// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Behavior

open System
open System.Threading

open IQ.Core.Data.Contracts


/// <summary>
/// Implements a builder pattern for <see cref="TabularDescription"/> values
/// </summary>
type TableDescriptionBuilder(schemaName, localName, doc) =
    let columns = ResizeArray<ColumnDescription>()
    let parentName = DataObjectName(schemaName, localName)
    let mutable colidx = -1

    let nextidx() =
        Interlocked.Increment(ref colidx)

    new(schemaName, localName) =
        TableDescriptionBuilder(schemaName, localName, String.Empty)
    
    new(tabularName : DataObjectName) =
        TableDescriptionBuilder(tabularName.SchemaName, tabularName.LocalName)

    member this.AddColumn(position, colname, dataTypeName, nullable, doc,  autoKind) =        
        let dataType = dataTypeName |> DataType.parse |> Option.get
        columns.Add({
                        Name = colname
                        Position = position
                        DataType = dataTypeName |> DataType.parse |> Option.get
                        DataKind = dataType |> DataProxyMetadata.getKind
                        Documentation = doc
                        Nullable = nullable
                        AutoValue = autoKind
                        ParentName = parentName
                        Properties = []

                        })
        this
    member this.AddColumn(colname, dataTypeName) = 
        this.AddColumn(nextidx(), colname, dataTypeName, false, String.Empty, AutoValueKind.None)

    member this.AddColumn(colname, dataTypeName, nullable) = 
        this.AddColumn(nextidx(), colname, dataTypeName, nullable, String.Empty, AutoValueKind.None)
    
    member this.AddColumn(colname, dataTypeName, nullable, doc) = 
        this.AddColumn(nextidx(), colname, dataTypeName, nullable, doc, AutoValueKind.None) 

    member this.Finish() =
            {
                TableDescription.Name = DataObjectName(schemaName, localName)
                Documentation = doc
                Columns = columns |> List.ofSeq
                Properties = []
                IsFileTable = false
            }
        

