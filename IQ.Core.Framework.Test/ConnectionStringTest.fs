namespace IQ.Core.Framework.Test

open IQ.Core.Framework
open IQ.Core.TestFramework

[<TestContainer>]
module ConnectionStringTest =
    
    [<Test>]
    let ``Formatted connection string``() =
        ["A"; "B"; "C"] |> ConnectionString |> ConnectionString.format |> Claim.equal "A;B;C"

    [<Test>]
    let ``Parsed connection string``() =
        "A;B;C" |> ConnectionString.parse |> Claim.equal (ConnectionString["A"; "B"; "C"])

