namespace IQ.Core.Framework.Test

open IQ.Core.Framework
open IQ.Core.TestFramework

[<TestContainer>]
module ConnectionStringTest =
    
    [<Fact>]
    let ``Formatted connection string``() =
        ["A"; "B"; "C"] |> ConnectionString |> ConnectionString.format |> Claim.equal "A;B;C"

    [<Fact>]
    let ``Parsed connection string``() =
        "A;B;C" |> ConnectionString.parse |> Claim.equal (ConnectionString["A"; "B"; "C"])

