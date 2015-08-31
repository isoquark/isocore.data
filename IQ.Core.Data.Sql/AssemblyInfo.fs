// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Data

open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

[<assembly: AssemblyTitle("IQ.Core.Data.Sql")>]
[<assembly: AssemblyDescription("")>]
[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCompany("eXaPhase")>]
[<assembly: AssemblyProduct("IQ.Core.Data.Sql")>]
[<assembly: AssemblyCopyright("Copyright © eXaPhase 2015")>]


[<assembly: AssemblyVersion("1.0.*")>]
[<assembly: AssemblyFileVersion("1.0.*")>]

[<assembly: InternalsVisibleTo("IQ.Core.Data.Sql.Test")>]

[<assembly: AutoOpen("IQ.Core.Data.Sql.Behavior")>]

do
    ()
