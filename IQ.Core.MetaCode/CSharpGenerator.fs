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
        
        let nameText = 
            if p.ValueType.FullName |> Option.isSome then 
                p.ValueType.FullName.Value 
            else 
                p.ValueType.SimpleName

        let typeName = if p.IsNullable then 
                            SF.NullableType(SF.ParseTypeName(nameText)) :> TypeSyntax 
                       else 
                            SF.ParseTypeName(nameText) 
        
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

//See this: http://roslynquoter.azurewebsites.net/
        
module internal ClrAttribution =
    let private arg(value : obj) = 
        SF.AttributeArgument(SF.LiteralExpression(SyntaxKind.StringLiteralExpression, SF.Literal(value :?> string)))
    
    let declare (a : ClrAttribution) =
       let argValues = match a.AppliedValues with | ValueIndex (x) -> x
       let args = argValues 
                |> List.sortBy(fun (x,y) -> x.Position) 
                |> List.map(fun (x,y) -> arg(y))
       let argList = SF.SeparatedList<AttributeArgumentSyntax>(args) |> SF.AttributeArgumentList
       SF.Attribute(SyntaxFactory.IdentifierName(a.AttributeName.SimpleName)).WithArgumentList(argList)
       
        
module internal Comment =
    let createToken syntaxKind text  =
        let format = sprintf "//%s" text
        let c = SF.TriviaList(SF.Comment(format))
        SF.Token(c, syntaxKind, SF.TriviaList())
        
module internal ClrClass =
   let private addModifiers modifiers (syntax : ClassDeclarationSyntax) =
        syntax.AddModifiers(modifiers)
   
   let private addMembers (members : MemberDeclarationSyntax seq) (syntax : ClassDeclarationSyntax) =
        syntax.AddMembers (members |> Array.ofSeq)

   let private createSummaryComment text =
        let format = sprintf "/// <summary> %s </summary>\r\n" text
        format |> SF.Comment |> SF.TriviaList

   let private addAttributes (attributions : ClrAttribution list) (syntax : ClassDeclarationSyntax) =
        attributions |> List.map(ClrAttribution.declare)
                     |> SF.SeparatedList  
                     |> SF.AttributeList //|> fun x -> x.WithLeadingTrivia (createSummaryComment "test") 
                     |> SF.SingletonList                     
                     |> syntax.WithAttributeLists
       

   let private addComments text (syntax : ClassDeclarationSyntax) =
        text |> createSummaryComment |> syntax.WithLeadingTrivia
       
   let declare (c : ClrClass) =
        let accessModifiers = c.Access |> ClrAccessKind.getKeywords |> Array.map SF.Token
        let partialModifier = SF.Token(SyntaxKind.PartialKeyword);
        let modifiers = [|partialModifier|] |> Array.append accessModifiers
        SF.ClassDeclaration(c.Name.SimpleName)
        |> addModifiers modifiers
        |> addAttributes c.Info.Attributes
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
        
    let comment (text : string) (cu : CompilationUnitSyntax) =
      text |> Comment.createToken SyntaxKind.EndOfFileToken |> cu.WithEndOfFileToken
               
    let using (name : string) (cu : CompilationUnitSyntax) =
    
        cu.AddUsings([|name |> MB.using|])

    let addType (t : TypeDeclarationSyntax) (cu : CompilationUnitSyntax) =
        cu.AddMembers([|t :> MemberDeclarationSyntax|])

    let addTypes (types : TypeDeclarationSyntax seq) (cu : CompilationUnitSyntax) =
        cu.AddMembers(
            types |> Seq.map(fun t -> t :> MemberDeclarationSyntax) 
                  |> Array.ofSeq)

    let addNamespace (t : NamespaceDeclarationSyntax) (cu : CompilationUnitSyntax) =
        cu.AddMembers([|t :> MemberDeclarationSyntax|])

    let addNamespaces (namespaces : NamespaceDeclarationSyntax seq) (cu : CompilationUnitSyntax) =
        cu.AddMembers(
            namespaces |> Seq.map(fun t -> t :> MemberDeclarationSyntax) 
                       |> Array.ofSeq
        )
    
    
module internal NS =
    let create (name : string)  =
        SF.NamespaceDeclaration(SF.IdentifierName(name))

    let addType (t : TypeDeclarationSyntax) (ns : NamespaceDeclarationSyntax) =
        ns.AddMembers([|t :> MemberDeclarationSyntax|])

    let addTypes (types : TypeDeclarationSyntax seq) (ns : NamespaceDeclarationSyntax) =
        ns.AddMembers(
            types |> Seq.map(fun t -> t :> MemberDeclarationSyntax) 
                  |> Array.ofSeq)
           
module internal PI =
    let create(name) =
        ProjectInfo.Create(ProjectId.CreateNewId(name), VersionStamp.Default, name, name, "C#", null, sprintf "%s.dll" name)

module internal WS =
    let create() = new AdhocWorkspace()


                        
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
    

    let genFile (dstFile : string) (types : ClrType list) =
        
        let namespaces = types |> List.groupBy(fun t -> t.TypeInfo.Namespace)
                               |> List.map(fun (nsname,types) ->
                                                    nsname |> NS.create
                                                           |> NS.addTypes(types |> List.map genType))
                
        let cu = CU.create() 
                             |> CU.comment (sprintf "Proxies generated on %O" BclDateTime.Now)
                             |> CU.using "System"
                             |> CU.using "System.Collections.Generic" 
                             |> CU.using "IQ.Core.Data.Contracts"
                             |> CU.addNamespaces namespaces
                
        use workspace = WS.create()

        let format = Formatter.Format(cu, workspace)
        let sb = StringBuilder()
        use writer = new StreamWriter(dstFile)
        format.WriteTo(writer)

    let genProject (dstFolder : string) (a : ClrAssembly) =
        
        let namespaces = a.Types |> List.groupBy(fun t -> t.TypeInfo.Namespace)
                                 |> List.map(fun (nsname,types) ->
                                                    nsname |> NS.create
                                                           |> NS.addTypes(types |> List.map genType))
        
        let cu = CU.create() |> CU.using "System"
                             |> CU.using "System.Collections.Generic" 
                             //|> CU.addTypes (a.Types |> List.map genType)
                             |> CU.addNamespaces namespaces
        
        
        use workspace = WS.create()

        let format = Formatter.Format(cu, workspace)
        let sb = StringBuilder()
        let path = Path.Combine(dstFolder, "Gen.cs")
        use writer = new StreamWriter(path)
        format.WriteTo(writer)
        
        let project = workspace.AddProject(a.Name.SimpleName, "C#")
        
        ()        

   

        
        
                    
