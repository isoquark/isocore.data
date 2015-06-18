namespace IQ.Core.Framework.Test

open IQ.Core.TestFramework
open IQ.Core.Framework

open System
open System.Reflection

[<TestContainer>]
module ClrAssemblyTest =
    [<Test>]
    let ``Extracted embedded text resource from assembly``() =
        let text = thisAssembly() |> Assembly.findTextResource "EmbeddedResource01.txt"
        text |> Claim.isSome
        text.Value.Trim() |> Claim.equal "This is an embedded text resource"


