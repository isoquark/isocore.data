// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.MetaCode.Shell

open System

open IQ.Core.Data
open IQ.Core.MetaCode
open IQ.Core.Framework;


module Main = 
    let inferProxyDescription (table : TableDescription) =
        let typeInfo = {
            ClrTypeInfo.Access = ClrAccessKind.Public
            Name = ClrTypeName(table.Name.Text, None, None)
            Position = 0
            ReflectedElement = None
            DeclaringType = None
            Kind = ClrTypeKind.Class
            IsOptionType = false
            Members = []
            IsStatic = false
            Attributes = []
            ItemValueType = ClrTypeName(table.Name.Text, None, None)
            Namespace = String.Empty
            DeclaredTypes = []
        }
        ClrClass(typeInfo)
    
    
    [<EntryPoint>]
    let main argv = 
        use context = new ShellContext()
        let cs = "csSqlDataStore" |> context.ConfigurationManager.GetValue 

        let dsProvider = context.AppContext.Resolve<IDataStoreProvider>()
        let store = dsProvider.GetDataStore<ISqlDataStore>(cs)
        let metadata = store.MetadataProvider

        let table = metadata.DescribeTable(DataObjectName("SqlTest", "Table02"))


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
