namespace IQ.Core.Data

open IQ.Core.Contracts

open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[<assembly: AssemblyTitle("IQ.Core.Data.Text")>]
[<assembly: AssemblyDescription("")>]
[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCompany("eXaPhase")>]
[<assembly: AssemblyProduct("IQ.Core.Data.Text")>]
[<assembly: AssemblyCopyright("Copyright © eXaPhase 2015")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: AssemblyCulture("")>]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[<assembly: ComVisible(false)>]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[<assembly: Guid("77511d20-eb21-4220-8939-cc0e5525da3a")>]

[<assembly: AssemblyVersion("1.0.*")>]
[<assembly: AssemblyFileVersion("1.0.*")>]

do
    ()

type TextDataAssemblyDescriptor() =
    inherit AssemblyDescriptor<TextDataAssemblyDescriptor>()
