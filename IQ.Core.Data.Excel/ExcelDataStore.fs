namespace IQ.Core.Data.Excel

open System.Collections.Generic;




module ExcelDataStore =
    let private rol(items : seq<_>) = List<_>(items) :> IReadOnlyList<_>
    
    type private Realization(cs) =
        interface IQueryableDataStore<ExcelDataStoreQuery> with
            member this.Select(q) =  
                match q with
                | WorksheetQuery(worksheetName) ->
                    rol[]
            member this.ConnectionString = ConnectionString([cs])
