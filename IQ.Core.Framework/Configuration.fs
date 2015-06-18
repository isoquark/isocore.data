namespace IQ.Core.Framework

open System
open System.Configuration

[<AutoOpen>]
module ConfigurationVocabulary =
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
/// Implements basic configuration management capabilities
/// </summary>
module Configuration =
    
    type private ConfigFileManager() =
        interface IConfigurationManager with
            member this.GetEnvironmentValue environment name =
                ConfigurationManager.AppSettings.[name] 
            member this.GetValue name =
                ConfigurationManager.AppSettings.[name]



    let getManager() =
        ConfigFileManager() :> IConfigurationManager    


