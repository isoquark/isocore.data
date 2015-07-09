namespace IQ.Core.Framework

/// <summary>
/// Defines the contract used by the application to retrieve and (eventually) specify
/// configuration settings
/// </summary>
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



