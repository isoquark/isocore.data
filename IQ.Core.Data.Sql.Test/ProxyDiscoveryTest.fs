namespace IQ.Core.Data.Sql.Test

open System
open System.ComponentModel
open System.Data
open System.Reflection

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Sql

[<TestContainer>]
module ``Sql Core Proxy Discovery`` =

    module SqlTest =    
        [<Description("SQL Test Table01")>]
        type Table01 = {
            [<Description("Col01 Description Text")>]
            
            [<StorageType(StorageKind.Int32)>]
            Col01 : uint16
            [<Description("Col02 Description Text")>]
            Col02 : int64 option
            [<Description("Col03 Description Text")>]
            Col03 : string
            [<Description("Col04 Description Text")>]
            Col04 : string option
            [<Description("Col05 Description Text")>]
            Col05 : string
        }
    

    [<Schema("SqlTest")>]
    module SqlTestProcedures =
        let private dataStore = Unchecked.defaultof<ISqlDataStore>
        
        type pTable01InsertInput = {
            Col02 : DateTime
            Col03 : int64
        }

        type pTable01InsertOutput = {
            Col01 : int
        }

    [<Schema("SqlTest")>]
    type ISqlTestRoutines =
        abstract pTable01Insert:col02 : DateTime -> col03 : int64 -> [<return : RoutineParameter("col01", ParameterDirection.Output)>] int
                              
    
    [<Test>]
    let ``Described [SqlTest].[pTable01Insert] procedure``() =
        let proxies = procproxies<ISqlTestRoutines>

        ()


