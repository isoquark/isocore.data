// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data.Contracts

open System


/// <summary>
/// Represents the intent to retrieve data from an Excel data store
/// </summary>
type ExcelDataStoreQuery =
    /// Retrieves the data contained within a named worksheet
    | WorksheetQuery of worksheetName : string
