namespace IQ.Core.Framework.Test
open System

open IQ.Core.Framework
open IQ.Core.TestFramework

[<TestContainer>]
module TypeTest =
    
    module private A = 
        type internal B = class end

        module internal C = 
            type internal D = class end

            module internal E = 
                type internal F = class end

    [<Test>]
    let ``Discovered nested types``() =
        let actualA = typeof<A.B>.DeclaringType |> Type.getNestedTypes
        let expectA = [typeof<A.B>; typeof<A.C.D>.DeclaringType] 
        actualA |> Claim.equal expectA

    [<Test>]
    let ``Loaded type from name``() =
        ClrTypeName("B", typeof<A.B>.FullName |> Some, None) |> Type.fromName |> Claim.equal typeof<A.B>

    [<Test>]
    let ``Determined the item value type of a type``() =        
        typeof<List<string>>.ItemValueType |> Claim.equal typeof<string>
        typeof<option<string>>.ItemValueType |> Claim.equal typeof<string>
        typeof<option<List<string>>>.ItemValueType |> Claim.equal typeof<string>
        typeof<string>.ItemValueType |> Claim.equal typeof<string>

