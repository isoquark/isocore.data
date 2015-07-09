namespace IQ.Core.Framework

open System
open System.Configuration

[<AutoOpen>]
module ConfigurationVocabulary = 
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


