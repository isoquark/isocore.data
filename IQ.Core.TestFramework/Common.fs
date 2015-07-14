namespace IQ.Core.TestFramework

open System
open System.Reflection

[<AutoOpen>]
module internal Commmon =
    type BclDateTime = System.DateTime

    let inline thisMethod() = MethodInfo.GetCurrentMethod() :?> MethodInfo

module Txt =
    let rightOfFirst (marker : string) (text : string) =
        let idx = text.IndexOf(marker) 
        if idx <> -1 then
            (idx + marker.Length) |> text.Substring
        else
            String.Empty

    let startsWith start (text : string) =
        start |> text.StartsWith
