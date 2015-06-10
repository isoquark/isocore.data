namespace IQ.Core.Data.Sql.Test

open System
open System.ComponentModel
open System.Data

open IQ.Core.TestFramework
open IQ.Core.Data
open IQ.Core.Data.Sql

[<TestContainer>]
module ``Sql Core Proxy Discovery`` =

    module SqlTest =    
        [<Description("SQL Test Table01")>]
        type Table01 = {
            [<Description("Col01 Description Text")>]
            [<Column(SqlDbType.Int)>]
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
    
    
    
    
    [<Test>]
    let ``Described table [SqlTest].[Table01] from proxy``() =
        let actual = tableinfo<SqlTest.Table01>

        ()


