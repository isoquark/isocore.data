// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.MetaCode

open Microsoft.CodeAnalysis;
open Microsoft.CodeAnalysis.Formatting

open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax

open IQ.Core.Framework


module Syntax =
    type private SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory
    
    let identifier (name : string) =
        SF.IdentifierName(name)

    let using (name : string) =
        SF.UsingDirective(name |> identifier)

    let declareClass (name : string) =
        SF.ClassDeclaration("name")

module CU =
    type private SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory

    let create() = 
        SF.CompilationUnit()

    let using (name : string) (cu : CompilationUnitSyntax) =
        cu.AddUsings([|name |> Syntax.using|])

    let addType (t : TypeDeclarationSyntax) (cu : CompilationUnitSyntax) =
        cu.AddMembers([|t :> MemberDeclarationSyntax|])

    let addTypes (types : TypeDeclarationSyntax seq) (cu : CompilationUnitSyntax) =
        cu.AddMembers(
            types |> Seq.map(fun t -> t :> MemberDeclarationSyntax) 
                  |> Array.ofSeq)
                        

module internal CSharpGenerator =
    let generateProject(a : ClrAssembly) =
        CU.create() |> CU.using "System" 
                    |> CU.using "System.Collections.Generic" 
