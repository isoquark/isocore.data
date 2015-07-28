// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.MetaCode

open System
open System.Reflection
open System.Reflection.Emit
open System.IO
open System.Text


open FSharp.Reflection
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SimpleSourceCodeServices


open IQ.Core.Framework


module StringBuilder =
    let append (text : String) (sb : StringBuilder) =
        text |> sb.Append |> ignore

    let appendLine (text : String) (sb : StringBuilder) =
        text |> sb.AppendLine |> ignore
        

type GenerationContext(builder) =    
        
    new () =
        GenerationContext(new StringBuilder())
    
    member val CurrentIndent = 0 with get, set
    member val CurrentNamespace = String.Empty with get, set
    member this.Builder : StringBuilder = builder
    member this.IndentText = if this.CurrentIndent = 0 then String.Empty else String.replicate this.CurrentIndent " "

module ClrEnum =
    let generate (context : GenerationContext) (e : ClrEnum) =
        let appendLine text = context.Builder |> StringBuilder.appendLine text
        let indent = context.IndentText
        sprintf "%stype %s ="  indent e.Name.SimpleName |>  appendLine
        let numericType = e.NumericType |> Type.get 

        e.Literals |> List.iter(fun x ->
            let literalValue = Convert.ChangeType(x.LiteralValue |> Option.get, numericType)
            sprintf "%s| %s = %A" indent  x.Name.Text literalValue |> appendLine
        )
          
module ClrType =
    
    module ClrAccessKind =    
        let generate(subject : ClrAccessKind) = function
            | ClrAccessKind.Public -> "public"
            | ClrAccessKind.Internal -> "internal"
            | ClrAccessKind.Private -> "private"
            | _ -> nosupport()

    let private genEnumLiteral (eb : EnumBuilder)  (f : ClrField) =
        let value = Convert.ChangeType(f.LiteralValue |> Option.get, eb.GetEnumUnderlyingType() )
        eb.DefineLiteral(f.Name.Text, value)        

    let private getBclVisibility (t : ClrType) =
            match t.DeclaringType with
            | Some(declarer) ->
                match t.Access with
                | ClrAccessKind.Internal -> TypeAttributes.NestedAssembly
                | ClrAccessKind.Private -> TypeAttributes.NestedPrivate
                | ClrAccessKind.Public -> TypeAttributes.NestedPublic
                | ClrAccessKind.ProtectedAndInternal -> TypeAttributes.NestedFamANDAssem
                | ClrAccessKind.ProtectedOrInternal -> TypeAttributes.NestedFamORAssem
                | ClrAccessKind.Protected -> TypeAttributes.NestedFamily
                | _ -> nosupport()
            | None ->
                match t.Access with
                | ClrAccessKind.Internal -> 
                    TypeAttributes.NotPublic
                | ClrAccessKind.Public -> 
                    TypeAttributes.Public
                | _ -> nosupport()


    
    let private genEnum (mb : ModuleBuilder) (e : ClrEnum) =
        let enumName = match e.Name.FullName with | Some(x) -> x | None -> e.Name.SimpleName
        let numericType = e.NumericType |> Type.get 
        let visibility = e |> EnumType |> getBclVisibility      
        let eb = mb.DefineEnum(enumName, visibility, numericType)
        e.Literals |> List.map(fun f -> f |> genEnumLiteral eb) |> ignore
        eb.CreateType()
        
    let internal generate0 (mb : ModuleBuilder) (t : ClrType) =
        match t with
        | EnumType(e) ->e |> genEnum mb
        | _ -> nosupport()

    let generate (context : GenerationContext) (t : ClrType) =
        if t.Info.Namespace <> context.CurrentNamespace then
            context.CurrentNamespace <- t.Info.Namespace
            context.CurrentNamespace |> sprintf "namespace %s"  |> StringBuilder.appendLine  <| context.Builder
        
        match t with
        | EnumType(e) ->e |> ClrEnum.generate context
        | _ -> nosupport()
        
type GenerationConfig = {
    OutputDirectory : string
}

exception ClrAssemblyGenerationError of FSharpErrorInfo list

module ClrAssemblyGenerator =
        
    let generate0(a : ClrAssembly) = 
        let assname = AssemblyName(a.Name.Text)
        let ab = AssemblyBuilder.DefineDynamicAssembly(assname, AssemblyBuilderAccess.RunAndSave)
        let dir = Environment.CurrentDirectory
        let filename = sprintf "%s.dll" assname.Name
        let path = Path.Combine(dir, filename)
        let mb = ab.DefineDynamicModule(assname.Name, filename)
        let buildType t = t |> ClrType.generate0 mb 
        let types = a.Types |> List.map buildType        
        ab.Save(filename)
        ab :> Assembly
        
    let generate (config : GenerationConfig) (a : ClrAssembly) = 
        let context = GenerationContext()
        a.Types |> List.iter(fun t -> ClrType.generate context t)         
        let fsFilePath = Path.ChangeExtension(Path.Combine(config.OutputDirectory, a.Name.SimpleName), "fs")
        File.WriteAllText(fsFilePath, context.Builder.ToString())
        let dllFilePath = Path.ChangeExtension(fsFilePath, "dll")
        
        let codeServices = SimpleSourceCodeServices()
        let errors, exitCode =
            codeServices.Compile([|"fsc.exe"; "-o"; dllFilePath; "-a"; fsFilePath|])
        if errors.Length <> 0 then
             errors |> List.ofSeq |> ClrAssemblyGenerationError |> raise        
        Assembly.LoadFrom(dllFilePath)
          



        
        
       
        
    

