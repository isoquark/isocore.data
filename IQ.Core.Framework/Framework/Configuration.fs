namespace IQ.Core.Framework

open System
open System.Configuration




[<AutoOpen>]
module ConfigurationVocabulary =
    /// <summary>
    /// Responsible for identifying a Data Store, Network Address or other resource
    /// </summary>
    type ConnectionString = ConnectionString of string list
    with
        /// <summary>
        /// The components of the connection string
        /// </summary>
        member this.Components = match this with ConnectionString(components) -> components


    type IConfigurationManager =
        /// <summary>
        /// Gets an identified configuration value for a specified environment
        /// </summary>
        /// <param name="environment">The name of the environment</param>
        /// <param name="name">The name of the value</param>
        abstract GetEnvironmentValue:environment:string->name:string->string

        /// <summary>
        /// Gets an identified configuration value for the environment named in the configuration file
        /// </summary>
        /// <param name="environment">The name of the environment</param>
        /// <param name="name">The name of the value</param>
        abstract GetValue:name:string->string

    /// <summary>
    /// Yes, the configuration manager has a config (!)
    /// </summary>
    type ConfigurationManagerConfig = {
        /// The name of the configuration manager
        Name : string
    }


/// <summary>
/// Implements basic configuration management capabilities
/// </summary>
module ConfigurationManager =
    
    type private ConfigFileManager(config : ConfigurationManagerConfig) =
        interface IConfigurationManager with
            member this.GetEnvironmentValue environment name =
                ConfigurationManager.AppSettings.[name] 
            member this.GetValue name =
                ConfigurationManager.AppSettings.[name]

    let get(config) =
        config |> ConfigFileManager :> IConfigurationManager    

module CoreConfiguration = 
    [<Literal>]
    let CoreServicesType = "IQ.Core.Framework.CoreServices"
    [<Literal>]
    let CoreRervicesMethod = "register"
    [<Literal>]
    let UserAssemblyPrefix = "IQ."

module ConnectionString =    
    /// <summary>
    /// Creates a ConnectionString instance from the supplied text
    /// </summary>
    /// <param name="text">The text to parse</param>
    let parse text =
        text |> Txt.split ";" |> List.ofArray |> ConnectionString    

    /// <summary>
    /// Renders the instance as semantic text that can subsequently be parsed
    /// </summary>
    /// <param name="cs">The connection string to format</param>
    let format (cs : ConnectionString) =
        cs.Components |> Txt.delemit ";"

[<AutoOpen>]
module ConnectionStringExtensions =
    type ConnectionString
    with
        member this.Text = this |> ConnectionString.format    