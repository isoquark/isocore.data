namespace IQ.Core.Framework.Test

open IQ.Core.Framework
open IQ.Core.TestFramework


open System
open System.Reflection

module ClrInterfaceTest =
    
    type private IInterfaceA = 
        abstract Method01:int->string->decimal
        abstract Method02: a : int -> b : string -> decimal

    let ``Described interface with non-tupled parameters and no inheritance``() =
        match typeref<IInterfaceA> with
        | InterfaceTypeReference(subject, members) ->
            subject.Name |> Claim.equal typeof<IInterfaceA>.ElementName
        | _ ->
            Claim.assertFail()

    type private IInterfaceB =
        abstract Method01:unit->unit
        abstract Method02:int->unit
        abstract Method03:int->int
        abstract Property01:DateTime
        abstract Property02:DateTime with get,set

    [<Test>]
    let ``Described interface with both property and method declarations``() =
        match typeref<IInterfaceB> with
        | InterfaceTypeReference(subject, members) -> 
            let methods = [for m in members do  match m with | MethodMemberReference(x) -> yield x | _ -> ()]
            methods.Length |> Claim.equal 3
            let m1 = methods |> List.find(fun x -> x.Name.Text = "Method01")
            m1.Return.ReturnType |> Claim.isNone
            m1.Parameters.Length |> Claim.equal 0

            let m2 = methods |> List.find(fun x -> x.Name.Text = "Method02")
            m2.Return.ReturnType |> Claim.isNone
            m2.Parameters.Length |> Claim.equal 1
            m2.Parameters.[0].Subject.Name.Text |> String.IsNullOrWhiteSpace |> Claim.isTrue
            m2.Parameters.[0].ParameterType |> Claim.equal typeof<int>
        | _ ->
            Claim.assertFail()
