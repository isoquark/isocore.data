namespace IQ.Core.Framework

open System
open System.Reflection

open Microsoft.FSharp.Reflection

/// <summary>
/// Defines System.Reflection.ParameterInfo helpers
/// </summary>
module ParameterInfo =
    let getAttribute<'T when 'T :> Attribute>(subject : ParameterInfo) =
        if Attribute.IsDefined(subject, typeof<'T>) then
            Attribute.GetCustomAttribute(subject, typeof<'T>) :?> 'T |> Some
        else
            None

    /// <summary>
    /// Gets all the attributes applied to parameter
    /// </summary>
    /// <param name="subject">The member whose attributes will be retrieved</param>
    let getAllAttributes(subject : ParameterInfo) =
        subject |> Attribute.GetCustomAttributes |> List.ofArray

/// <summary>
/// Defines <see cref="System.Reflection.MemberInfo"/> helpers
/// </summary>
module internal MemberInfo =
    /// <summary>
    /// Retrieves an attribute applied to a member, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttribute<'T when 'T :> Attribute>(subject : MemberInfo) =
        if Attribute.IsDefined(subject, typeof<'T>) then
            Attribute.GetCustomAttribute(subject, typeof<'T>) :?> 'T |> Some
        else
            None    

    /// <summary>
    /// Retrieves an arbitrary number of attributes of the same type applied to a member
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttributes<'T when 'T :> Attribute>(subject : MemberInfo) =
        [for a in Attribute.GetCustomAttributes(subject, typeof<'T>) -> a :?> 'T]
        

    /// <summary>
    /// Gets all the attributes applied to a specified member
    /// </summary>
    /// <param name="subject">The member whose attributes will be retrieved</param>
    let getAllAttributes(subject : MemberInfo) =
        subject |> Attribute.GetCustomAttributes |> List.ofArray


/// <summary>
/// Defines System.MethodInfo helpers
/// </summary>
module MethodInfo =
    
    /// <summary>
    /// Retrieves an attribute applied to a member, if present
    /// </summary>
    /// <param name="subject">The method to examine</param>
    let getAttribute<'T  when 'T :> Attribute>(subject : MethodInfo) =
        subject |> MemberInfo.getAttribute<'T>

       
    /// <summary>
    /// Determines whether an attribute has been applied to a method
    /// </summary>
    /// <param name="subject">The method to examine</param>
    let hasAttribute<'T when 'T :> Attribute>(subject : MethodInfo) =
        Attribute.IsDefined(subject, typeof<'T>)

    /// <summary>
    /// Retrieves an attribute applied to a method return, if present
    /// </summary>
    /// <param name="subject">The method to examine</param>
    let getReturnAttribute<'T when 'T :> Attribute>(subject : MethodInfo) =
        let attribs = subject.ReturnTypeCustomAttributes.GetCustomAttributes(typeof<'T>, true)
        if attribs.Length <> 0 then
            attribs.[0] :?> 'T |> Some
        else
            None
        
module PropertyInfo =
    /// <summary>
    /// Retrieves an attribute applied to a property, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttribute<'T when 'T :> Attribute>(subject : PropertyInfo) = 
        subject |> MemberInfo.getAttribute<'T>

    /// <summary>
    /// Gets the data type of the property, ignoring whether the property is optional
    /// </summary>
    /// <param name="p">The property</param>
    let getValueType (p : PropertyInfo) =
        p.PropertyType |>  Type.getItemValueType


module FieldInfo =
    /// <summary>
    /// Retrieves an attribute applied to a property, if present
    /// </summary>
    /// <param name="subject">The type to examine</param>
    let getAttribute<'T when 'T :> Attribute>(subject : FieldInfo) = 
        subject |> MemberInfo.getAttribute<'T>

    /// <summary>
    /// Gets the data type of the property, ignoring whether the property is optional
    /// </summary>
    /// <param name="f">The field</param>
    let getValueType (f : FieldInfo) =
        f.FieldType |> Type.getItemValueType


/// <summary>
/// Defines <see cref="Microsoft.FSharp.Reflection.UnionCaseInfo"/> helpers
/// </summary>
module UnionCaseInfo =
    /// <summary>
    /// Retrieves identified custom attribute if applied
    /// </summary>
    let getAttribute<'T when 'T :> Attribute>(subject : UnionCaseInfo) =
        let attribs = subject.GetCustomAttributes(typeof<'T>)
        if attribs.Length <> 0 then
            attribs.[0] :?> 'T |> Some
        else
            None

