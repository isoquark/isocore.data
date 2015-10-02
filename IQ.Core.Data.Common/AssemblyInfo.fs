namespace IQ.Core.Data


open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

open IQ.Core.Contracts

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[<assembly: AssemblyTitle("IQ.Core.Data")>]
[<assembly: AssemblyDescription("")>]
[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCompany("eXaPhase")>]
[<assembly: AssemblyProduct("IQ.Core.Data")>]
[<assembly: AssemblyCopyright("Copyright © Microsoft 2015")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: AssemblyCulture("")>]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[<assembly: ComVisible(false)>]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[<assembly: Guid("5d4ada5c-b453-432c-a419-81bf001edd14")>]

// Version information for an assembly consists of the following four values:
// 
//       Major Version
//       Minor Version 
//       Build Number
//       Revision
// 
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [<assembly: AssemblyVersion("1.0.*")>]
[<assembly: AssemblyVersion("1.0.*")>]
[<assembly: AssemblyFileVersion("1.0.*")>]

[<assembly: AutoOpen("IQ.Core.Data.Behavior")>]
do
    ()

type DataAssemblyDescriptor() =
    inherit AssemblyDescriptor<DataAssemblyDescriptor>()
