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

    [<Test>]
    let ``Removed character from text``() =
        let input = "'41816141-0dbd-46c6-ab55-6037a1da8790'"
        let expect = "41816141-0dbd-46c6-ab55-6037a1da8790"
        let actual = input |> Txt.removeChar '''
        expect |> Claim.equal actual
                    
