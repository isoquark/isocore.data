namespace IQ.Core.Data

open System
open System.Linq
open System.Collections.Generic;
open System.Collections.Concurrent;

open System.Data
open System.Data.SqlClient


open IQ.Core.Contracts
open IQ.Core.Framework

type SqlMetadataIndex(catalog : SqlMetadataCatalog) =
        let badargs() = ArgumentException() |> raise
        
        let schemaObjects = Dictionary<string, Dictionary<DataElementKind, List<DataObjectDescription>>>()
        let allObjects = Dictionary<DataObjectName, DataObjectDescription>()

        let indexObject(o : DataObjectDescription) =
            allObjects.[o.ObjectName] <- o
            let schemaName = o.ObjectName.SchemaName
            let schemaIndex =
                if schemaObjects.ContainsKey(schemaName) then
                    schemaObjects.[schemaName]
                else
                    schemaObjects.[schemaName] <- Dictionary<DataElementKind, List<DataObjectDescription>>()
                    schemaObjects.[schemaName]
            if schemaIndex.ContainsKey(o.ElementKind) then
                schemaIndex.[o.ElementKind].Add(o)
            else
                schemaIndex.[o.ElementKind] <- List([o])

        do
            for s in catalog.Schemas do
                for o in s.Objects do
                    o |> indexObject

        let getSchemaObjects elementKind schemaName =
            if schemaObjects.[schemaName].ContainsKey(elementKind) then
                schemaObjects.[schemaName].[elementKind] :> seq<_>
            else
                Seq.empty            
        
        /// <summary>
        /// Retrieves an identified table
        /// </summary>
        member this.GetTable(name : DataObjectName) =
            match allObjects.[name] with
            | TableDescription(x) -> x
            | _ -> badargs()

        /// <summary>
        /// Retrieves views defined in an identified schema
        /// </summary>
        member this.GetSchemaTables(schemaName) =
            schemaName |> getSchemaObjects DataElementKind.Table
                        |> Seq.map(fun x -> match x with | TableDescription(x) -> x | _ -> badargs())
                        |> List.ofSeq                


        /// <summary>
        /// Retrieves an identified view
        /// </summary>
        member this.GetView(name : DataObjectName) =
            match allObjects.[name] with
            | ViewDescription(x) -> x
            | _ -> badargs()

        /// <summary>
        /// Retrieves views defined in an identified schema
        /// </summary>
        member this.GetSchemaViews(schemaName) =
            schemaName |> getSchemaObjects DataElementKind.View 
                        |> Seq.map(fun x -> match x with | ViewDescription(x) -> x | _ -> badargs())
                        |> List.ofSeq                

        /// <summary>
        /// Retrieves an identified procedure
        /// </summary>
        member this.GetProcedure(name : DataObjectName) =
            match allObjects.[name] with
            | RoutineDescription(x) -> 
                if x.RoutineKind <> DataElementKind.Procedure then
                    badargs()
                x
            | _ -> badargs()
            
        /// <summary>
        /// Retrieves an identified table-valued funciton
        /// </summary>
        member this.GetTableFunction(name : DataObjectName) =
            match allObjects.[name] with
            | RoutineDescription(x) -> 
                if x.RoutineKind <> DataElementKind.TableFunction then
                    badargs()
                x
            | _ -> badargs()

        /// <summary>
        /// Retrieves an identified data type
        /// </summary>
        member this.GetDataType(name : DataObjectName) =
            match allObjects.[name] with
            | DataTypeDescription(x) ->
                x
            | _ -> badargs()
    
        /// <summary>
        /// Retrieves data types defined in an identified schema
        /// </summary>
        member this.GetSchemaDataTypes(schema : string) =
            schema |> getSchemaObjects DataElementKind.DataType 
                   |> Seq.map(fun x -> match x with | DataTypeDescription(x) -> x | _ -> nosupport())
    
        /// <summary>
        /// Retrieves an identified sequence
        /// </summary>
        member this.GetSequence(name : DataObjectName) =
            match allObjects.[name] with
            | SequenceDescription(x) ->
                x
            | _ -> badargs()
    


