namespace IQ.Core.Data.Sql.Test

open System
open System.ComponentModel
open System.Data
open System.Data.SqlClient
open System.Reflection

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Sql
open IQ.Core.Framework


    


[<TestContainer>]
module ``Proxy Discovery`` =
                                              
    [<Test>]
    let ``Described [SqlTest].[pTable02Insert] procedure from proxy``() =
        let procName = thisMethod() |> SqlTestCaseMethod.getDbObjectName
        let proxies = procproxies<SqlTestProxies.ISqlTestProcs>
        let proxy = proxies |> List.find(fun x -> x.DataElement.Name = procName)
        let proc = proxy.DataElement

        proc.Name |> Claim.equal procName
        proc.Parameters.Length |> Claim.equal 3

        let param01 = proc.FindParameter "col01"
        param01.Direction |> Claim.equal ParameterDirection.Output
        param01.StorageType |> Claim.equal Int32Storage
        
        let param02 = proc.FindParameter "col02"
        param02.Direction |> Claim.equal ParameterDirection.Input
        param02.StorageType |> Claim.equal (DateTimeStorage(7uy))

        let param03 = proc.FindParameter "col03"
        param03.Direction |> Claim.equal ParameterDirection.Input
        param03.StorageType |> Claim.equal Int64Storage        
        

        
