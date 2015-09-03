// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Math

open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

open IQ.Core.Contracts

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[<assembly: AssemblyTitle("IQ.Core.Math")>]
[<assembly: AssemblyDescription("")>]
[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCompany("Microsoft")>]
[<assembly: AssemblyProduct("IQ.Core.Math")>]
[<assembly: AssemblyCopyright("Copyright © Microsoft 2015")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: AssemblyCulture("")>]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[<assembly: ComVisible(false)>]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[<assembly: Guid("d01a6d75-53f1-4814-be92-5f69113ad1b9")>]

[<assembly: AssemblyVersion("1.0.*")>]
[<assembly: AssemblyFileVersion("1.0.*")>]

do
    ()

type MathAssemblyDescriptor() =
    inherit AssemblyDescriptor<MathAssemblyDescriptor>()
