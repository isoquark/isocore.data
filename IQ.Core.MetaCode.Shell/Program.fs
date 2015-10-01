// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.MetaCode.Shell


open IQ.Core.Data


open IQ.Core.MetaCode

open IQ.Core.Framework;


module Main = 
    [<EntryPoint>]
    let main argv = 
        use context = new ShellContext()
        let cs = "csSqlDataStore" |> context.ConfigurationManager.GetValue 
        let storeConfig = SqlDataStoreConfig(cs)
        let store : ISqlDataStore = storeConfig |> context.AppContext.Resolve

    
        let prop = {
            Name = ClrMemberName("Prop1")
            Position = 0
            ReflectedElement = None
            Attributes = []
            GetMethodAttributes = []
            SetMethodAttributes = []
            DeclaringType = ClrTypeName("MyClass1", None, None)
            ValueType = ClrTypeName("Int32", Some("System.Int32"), None)
            IsOptional = false
            IsNullable = false
            CanRead = true
            ReadAccess = ClrAccessKind.Public |> Some
            CanWrite = true
            WriteAccess = ClrAccessKind.Public |> Some
            IsStatic = false
        }

        let info = {
            Name = ClrTypeName("MyClass1", None, None)
            Position = 0
            ReflectedElement = None
            DeclaringType = None
            DeclaredTypes = []
            Kind = ClrTypeKind.Class
            IsOptionType = false
            Members = [prop |> PropertyMember]
            Access = ClrAccessKind.Public
            IsStatic = false
            Attributes = []
            ItemValueType = ClrTypeName("MyClass1", None, None)
            Namespace = "IQ.Types"
        }



        let assembly = 
            {
                Name = ClrAssemblyName("IQ.Types", None)
                ReflectedElement = None
                Position = 0
                Types = [ClrClass(info) |> ClassType]
                Attributes = []
                References = []
            }

        assembly |> CSharpGenerator.genProject @"C:\Temp"

        0 
