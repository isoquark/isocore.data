namespace IQ.Core.Framework

open System
open System.Reflection
open System.IO

/// <summary>
/// Defines operations for working with CLR assemblies
/// </summary>
module ClrAssembly =
    /// <summary>
    /// Retrieves a text resource embedded in the subject assembly if found
    /// </summary>
    /// <param name="shortName">The name of the resource, excluding the namespace path</param>
    /// <param name="subject">The assembly that contains the resource</param>
    let findTextResource shortName (subject : Assembly) =        
        match subject.GetManifestResourceNames() |> Array.tryFind(fun name -> name.Contains(shortName)) with
        | Some(resname) ->
            use s = resname |> subject.GetManifestResourceStream
            use r = new StreamReader(s)
            r.ReadToEnd() |> Some
        | None ->
            None

    /// <summary>
    /// Writes a text resource contained in an assembly to a file and returns the path
    /// </summary>
    /// <param name="shortName">The name of the resource, excluding the namespace path</param>
    /// <param name="outputDir">The directory into which the resource will be deposited</param>
    /// <param name="subject">The assembly that contains the resource</param>
    let writeTextResource shortName outputDir (subject : Assembly) =
        let path = Path.ChangeExtension(outputDir, shortName) 
        match subject |> findTextResource shortName with
        | Some(text) -> File.WriteAllText(path, text)
        | None ->
            ArgumentException(sprintf "Resource %s not found" shortName) |> raise
        path
        
/// <summary>
/// Defines Assembly-related augmentations
/// </summary>
[<AutoOpen>]
module ClrAssemblyExtensions =
    /// <summary>
    /// Defines augmentations for the Assembly type
    /// </summary>
    type Assembly
    with
        /// <summary>
        /// Gets the short name of the assembly without version/culture/security information
        /// </summary>
        member this.ShortName = this.GetName().Name    

