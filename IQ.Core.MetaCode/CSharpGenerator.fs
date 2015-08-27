// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.MetaCode

open System
open System.Text;
open System.IO;

open Microsoft.CodeAnalysis;
open Microsoft.CodeAnalysis.Formatting

open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax

open IQ.Core.Framework

type internal SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory

type LanguageKind =
    | FSharp = 1uy
    | CSharp = 2uy

type UnsupportedConstruct(language, construct) =
    inherit Exception()
    member this.Language : LanguageKind = language
    member this.Construct : string = construct

    override this.ToString() =
        sprintf "The %A language does not support the %s construct" language construct


module internal ClrAccessKind =
    let getKeywords(access : ClrAccessKind) =
        match access with
        | ClrAccessKind.Public -> 
            [|SyntaxKind.PublicKeyword|]

        | ClrAccessKind.Protected -> 
            [|SyntaxKind.ProtectedKeyword|]
        
        | ClrAccessKind.Private -> 
            [|SyntaxKind.PrivateKeyword|]
        
        | ClrAccessKind.Internal -> 
            [|SyntaxKind.InternalKeyword|]
        
        | ClrAccessKind.ProtectedOrInternal -> 
            [|SyntaxKind.ProtectedKeyword; SyntaxKind.InternalKeyword|]
        | _ -> nosupport()


module internal ClrProperty =        
    let private addModifiers modifiers (syntax : PropertyDeclarationSyntax) =
        syntax.AddModifiers(modifiers)
    
    let private addAccessor accessor (syntax : PropertyDeclarationSyntax) =
        syntax.AddAccessorListAccessors([|accessor|])

    let private addAutoGetAccessor (syntax : PropertyDeclarationSyntax) =       
        addAccessor <| SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)) 
                    <| syntax

    let private addAutoSetAccessor (syntax : PropertyDeclarationSyntax) =
        addAccessor <| SF.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken))
                    <| syntax

    let declare(p : ClrProperty) =
        if not(p.CanRead && p.CanWrite) then
            nosupport() 
        if p.ReadAccess <> p.WriteAccess then
            nosupport() 
        
        let accessModifiers = p.ReadAccess.Value |> ClrAccessKind.getKeywords |> Array.map SF.Token

        let typeName = SF.ParseTypeName(p.ValueType.SimpleName)
        SF.PropertyDeclaration(typeName, p.Name.Text) 
        |> addModifiers accessModifiers
        |> addAutoGetAccessor
        |> addAutoSetAccessor


module internal ClrMember=
    let declare (m : ClrMember) =
        match m with
        | PropertyMember(p) ->
            p |> ClrProperty.declare :> MemberDeclarationSyntax
        
        | FieldMember(m) ->
            UnsupportedConstruct(LanguageKind.CSharp, "field") |> raise
        
        | MethodMember(m) ->
            nosupport()
        
        | EventMember(m) ->
            nosupport()
        
        | ConstructorMember(m) ->
            nosupport()
        
        
module internal ClrClass =        
   let private addMembers (members : MemberDeclarationSyntax seq) (syntax : ClassDeclarationSyntax) =
        syntax.AddMembers (members |> Array.ofSeq)
    
   let declare (c : ClrClass) =
        SF.ClassDeclaration(c.Name.SimpleName)
        |> addMembers (List.map ClrMember.declare <| c.Info.Members )

                             
/// <summary>
/// Model builder
/// </summary>
module internal MB =
    
    let identifier (name : string) =
        SF.IdentifierName(name)

    let using (name : string) =
        SF.UsingDirective(name |> identifier)
                   

module internal CU =
    type private SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory

    let create() = 
        SF.CompilationUnit()

    let using (name : string) (cu : CompilationUnitSyntax) =
        cu.AddUsings([|name |> MB.using|])

    let addType (t : TypeDeclarationSyntax) (cu : CompilationUnitSyntax) =
        cu.AddMembers([|t :> MemberDeclarationSyntax|])

    let addTypes (types : TypeDeclarationSyntax seq) (cu : CompilationUnitSyntax) =
        cu.AddMembers(
            types |> Seq.map(fun t -> t :> MemberDeclarationSyntax) 
                  |> Array.ofSeq)
                        
module CSharpGenerator =

    
    let private genType (t : ClrType) =
        match t with
        | ClassType(t) -> 
            t |> ClrClass.declare  :> TypeDeclarationSyntax            
        
        | EnumType(t) -> 
            nosupport()
        
        | ModuleType(t) -> 
            UnsupportedConstruct(LanguageKind.CSharp, "module") |> raise
        
        | CollectionType(t) -> 
            nosupport()
        
        | StructType(t) -> nosupport()
        
        | UnionType(t) -> 
            UnsupportedConstruct(LanguageKind.CSharp, "union") |> raise
        
        | RecordType(t) -> 
            UnsupportedConstruct(LanguageKind.CSharp, "record") |> raise
        
        | InterfaceType(t) -> 
            nosupport()    
    

    let genProject (dstFolder : string) (a : ClrAssembly) =
        let cu = CU.create() |> CU.using "System"  
                             |> CU.using "System.Collections.Generic" 
                             |> CU.addTypes (a.Types |> List.map genType)
        use workspace = new AdhocWorkspace()
        //ProjectInfo.Create(ProjectId.CreateNewId("MyTestProjectId"), VersionStamp.Create(), "MyProject.dll", "C#");
        let format = Formatter.Format(cu, workspace)
        let sb = StringBuilder()
        let path = Path.Combine(dstFolder, "Gen.cs")
        use writer = new StreamWriter(path)
        format.WriteTo(writer)
        
   

        
        
                    
