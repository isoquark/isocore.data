namespace IQ.Core.Framework.Test

open XUnit

type TxtTest(ctx,log) =
    inherit ProjectTestContainer(ctx,log)
    
    [<Fact>]
    let ``Found text to the right of a marker``()  =
        "ABCDE-FGH" |> Txt.rightOfFirst "-" |> Claim.equal "FGH"


    [<Fact>]
    let ``Matched regular expression groups``() =
        let regex = @"(?<StorageName>[a-zA-z]*)\((?<Length>[0-9]*)\)"
        let values = "AnsiTextVariable(5)" |> Txt.matchRegexGroups ["StorageName"; "Length"] regex
        values.["StorageName"] |> Claim.equal "AnsiTextVariable"
        values.["Length"] |> Claim.equal "5"
        ()

    [<Fact>]
    let ``Removed character from text``() =
        let input = "'41816141-0dbd-46c6-ab55-6037a1da8790'"
        let expect = "41816141-0dbd-46c6-ab55-6037a1da8790"
        let actual = input |> Txt.removeChar '''
        expect |> Claim.equal actual

    [<Fact>]
    let ``Created delimited list``() =
        ["1"; "2"; "3"] |> Txt.delemit "," |> Claim.equal "1,2,3"
        ["1"; "2"; "3"] |> Txt.delemit "**" |> Claim.equal "1**2**3"

    [<Fact>]
    let ``Found text beween two indices``() =
        let input = "0123456789"
        input |> Txt.betweenIndices 4 6 true |> Claim.equal "456"
        input |> Txt.betweenIndices 3 7 false |> Claim.equal "456"

    [<Fact>]
    let ``Found text between two markers``() =
        let input = "Executed [SqlTest].[pTable02Insert] procedure"
        input |> Txt.betweenMarkers "[" "]" true |> Claim.equal "[SqlTest].[pTable02Insert]"
        input |> Txt.betweenMarkers "[" "]" false |> Claim.equal "SqlTest].[pTable02Insert"