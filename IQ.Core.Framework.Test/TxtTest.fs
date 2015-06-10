namespace IQ.Core.Framework.Test

open IQ.Core.Framework
open IQ.Core.TestFramework

[<TestContainer>]
module TxtTest =
    
    [<Test>]
    let ``Found text to the right of a marker``()  =
        "ABCDE-FGH" |> Txt.rightOf "-" |> Claim.equal "FGH"

