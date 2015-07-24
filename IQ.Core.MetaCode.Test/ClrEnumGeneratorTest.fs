// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.MetaCode.Test

open System
open System.Reflection
open System.Reflection.Emit
open System.Collections

open FSharp.Reflection



open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.MetaCode


open IQ.Core.MetaCode.Test.Prototypes

module ClrEnumGenerator =

        

    type LogicTests(ctx,log) =
        inherit ProjectTestContainer(ctx,log)
                 
        let describeAssembly simpleName types =
                {
                    Name = ClrAssemblyName(simpleName, None)
                    ReflectedElement = None
                    Position = 0
                    Types = types
                    Attributes = []
                    References = []
                }

        let generateAssembly types =
            let simpleName = "IQ.MetaCode.Test.Prototypes.Enums"
            let config = {GenerationConfig.OutputDirectory = ctx.OutputDirectory}
            describeAssembly simpleName types |> ClrAssemblyGenerator.generate config            
            
        let verifyEnum (expect : ClrType) (actual : ClrType) =
            actual.Name.FullName |> Claim.equal expect.Name.FullName
            actual.Fields.Length |> Claim.equal expect.Fields.Length
            match (expect,actual) with
            | (EnumType(input), EnumType(output)) ->
                output.NumericType |> Claim.equal input.NumericType
            | _ -> 
                Claim.assertFalse()                                              
            


        [<Fact>]
        let ``Generated Enums``() =
            let inputs = [typeinfo<Enum1>; typeinfo<Enum2>]
            let assembly = generateAssembly inputs
            let provider = ClrMetadataProvider.get {Assemblies = [assembly.AssemblyName]}
            let outputs = inputs |> List.map(fun x -> x, x.Name |> provider.FindType ) 
            outputs |> List.iter(fun (expect, actual) -> verifyEnum expect actual)
            
           
        
            
                
    
            

