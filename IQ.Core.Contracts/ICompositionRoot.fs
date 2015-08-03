// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System


/// <summary>
/// Type alias for delegate that produces configured service realizations
/// </summary>
type ServiceFactory<'TConfig,'I> = 'TConfig->'I
    
/// <summary>
/// Defines contract to allow items/factories to be registered with the container
/// </summary>
type ICompositionRegistry =
    /// <summary>
    /// Registers an instance value
    /// </summary>
    abstract RegisterInstance<'I> : 'I->unit when 'I : not struct
        
    /// <summary>
    /// Registers the interfaces implemented by the provided type
    /// </summary>
    abstract RegisterInterfaces<'T> : unit->unit

    /// <summary>
    /// Registers a service factory method
    /// </summary>
    abstract RegisterFactory<'TConfig, 'I> : ServiceFactory<'TConfig,'I> -> unit when 'I : not struct

    /// <summary>
    /// Registers a service factory method from a delegate (to place nice with C#)
    /// </summary>
    abstract RegisterFactoryDelegate<'TConfig, 'I> : Func<'TConfig,'I> ->unit when 'I : not struct

/// <summary>
/// Defines contract for an application execution context for a given container/root
/// </summary>
type IAppContext =
    inherit IDisposable
        
    /// <summary>
    /// Resoves contract that requires no resolution-time configuration
    /// </summary>
    abstract Resolve<'T> :unit->'T
        
    /// <summary>
    /// Resolves contract by finding a constructor on the type with a parameter name 'key' and passing 
    /// the value to it when instantiating it. This should be used sparingly, if ever
    /// </summary>
    /// <param name="key">The name of the parameter</param>
    /// <param name="value">The value to inject</param>
    abstract Resolve<'T> : key :string * value : obj->'T
        
    /// <summary>
    /// Resoves contract using supplied configuration
    /// </summary>
    abstract Resolve<'C,'I> : config : 'C -> 'I


/// <summary>
/// Defines the contract for the composition root in the DI pattern
/// </summary>
type ICompositionRoot =        
    inherit IDisposable
    inherit ICompositionRegistry
    
    /// <summary>
    /// Call when all dependencies have been specified and the container should
    /// be readied for use
    /// </summary>
    abstract Seal:unit->unit
    
    /// <summary>
    /// Creates a context within which to resolve depencies; resolved dependencies
    /// are disposed when the context is disposed
    /// </summary>
    abstract CreateContext:unit -> IAppContext
    