namespace IQ.Core.Framework

open System
open System.Reflection
open System.IO

/// <summary>
/// Defines System.Assembly helpers
/// </summary>
module Assembly =
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
    /// Determines whether a named assembly has been loaded
    /// </summary>
    /// <param name="name">The name of the assembly</param>
    let isLoaded (name : AssemblyName) =
        AppDomain.CurrentDomain.GetAssemblies() 
            |> Array.map(fun a -> a.GetName()) 
            |> Array.exists (fun n -> n = name)
    
    /// <summary>
    /// Recursively loads assembly references into the application domain
    /// </summary>
    /// <param name="subject">The staring assembly</param>
    let rec loadReferences (filter : string option) (subject : Assembly) =
        let references = subject.GetReferencedAssemblies()
        let filtered = match filter with
                        | Some(filter) -> 
                            references |> Array.filter(fun x -> x.Name.StartsWith(filter)) 
                        | None ->
                            references

        filtered |> Array.iter(fun name ->
            if name |> isLoaded |>not then
                name |> AppDomain.CurrentDomain.Load |> loadReferences filter
        )


        





            

