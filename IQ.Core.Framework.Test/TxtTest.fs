namespace IQ.Core.Framework.Test

open IQ.Core.Framework
open IQ.Core.TestFramework

[<TestContainer>]
module TxtTest =
    
    [<Test>]
    let ``Found text to the right of a marker``()  =
        "ABCDE-FGH" |> Txt.rightOf "-" |> Claim.equal "FGH"


    [<Test>]
    let ``Matched regular expression groups``() =
        let regex = @"(?<StorageName>[a-zA-z]*)\((?<Length>[0-9]*)\)"
        let values = "AnsiTextVariable(5)" |> Txt.matchRegexGroups ["StorageName"; "Length"] regex
        values.["StorageName"] |> Claim.equal "AnsiTextVariable"
        values.["Length"] |> Claim.equal "5"
        ()
        
