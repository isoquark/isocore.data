namespace IQ.Core.Framework

[<AutoOpen>]
module Lang =
    /// <summary>
    /// Alias for a KVP map
    /// </summary>
    type ValueMap = Map<string,obj>

    /// <summary>
    /// Responsible for identifying a Data Store, Network Address or other resource
    /// </summary>
    type ConnectionString = ConnectionString of string list

