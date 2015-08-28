// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System
open System.Threading

/// <summary>
/// Implements a builder pattern for <see cref="TabularDescription"/> values
/// </summary>
type TabularDescriptionBuilder(schemaName, localName, doc) =
    let columns = ResizeArray<ColumnDescription>()

    let mutable colidx = -1

    let nextidx() =
        Interlocked.Increment(ref colidx)

    new(schemaName, localName) =
        TabularDescriptionBuilder(schemaName, localName, String.Empty)
    
    new(tabularName : DataObjectName) =
        TabularDescriptionBuilder(tabularName.SchemaName, tabularName.LocalName)

    member this.AddColumn(position, colname, dataTypeName, nullable, doc,  autoKind) =        
        columns.Add({
                        Name = colname
                        Position = position
                        StorageType = dataTypeName |> DataType.parse |> Option.get
                        Documentation = doc
                        Nullable = nullable
                        AutoValue = autoKind})
        this
    member this.AddColumn(colname, dataTypeName) = 
        this.AddColumn(nextidx(), colname, dataTypeName, false, String.Empty, AutoValueKind.None)

    member this.AddColumn(colname, dataTypeName, nullable) = 
        this.AddColumn(nextidx(), colname, dataTypeName, nullable, String.Empty, AutoValueKind.None)
    
    member this.AddColumn(colname, dataTypeName, nullable, doc) = 
        this.AddColumn(nextidx(), colname, dataTypeName, nullable, doc, AutoValueKind.None) 

    member this.Finish() =
            {
                TabularDescription.Name = DataObjectName(schemaName, localName)
                Documentation = doc
                Columns = columns 
            }
        
