namespace IQ.Core.Framework

open System
open System.Reflection
open System.IO

module Member =
    let getKind (m : MemberInfo) =
        match m with
        | :? EventInfo ->ClrMemberKind.Event
        | :? MethodInfo -> ClrMemberKind.Method
        | :? PropertyInfo -> ClrMemberKind.Property
        | :? FieldInfo -> ClrMemberKind.StorageField
        | :? ConstructorInfo -> ClrMemberKind.Constructor
        | _ -> nosupport()

[<AutoOpen>]
module MemberExtensions = 

    /// <summary>
    /// Gets the currently executing method (not to be used for constructors!)
    /// </summary>
    /// <remarks>
    /// Note that since the method is designated inline, the call to get the executing
    /// method is injected at the call-site and so works as expected
    /// </remarks>
    let inline thisMethod() = MethodInfo.GetCurrentMethod() :?> MethodInfo

    type PropertyInfo
    with
        member this.ValueType = this.PropertyType |> Type.getItemValueType

    type FieldInfo
    with
        member this.ValueType = this.FieldType |> Type.getItemValueType
