// Copyright (c) Chris Moore and eXaPhase Consulting LLC.  All Rights Reserved.  Licensed under 
// the Apache License, Version 2.0.  See License.txt in the project root for license information.
namespace IQ.Core.Framework

open System
open System.Reflection

open Castle
open Castle.DynamicProxy


/// <summary>
/// Provides the capability to dynamically realize a contract
/// </summary>
module DynamicContract =
    let realize<'TContract, 'TConfig when 'TContract : not struct>(config : 'TConfig) (f:'TConfig->MethodInfo->obj[]->obj option) =
        let realization =
            {new IInterceptor with
                member this.Intercept(invocation) =
                    match f config invocation.Method invocation.Arguments with
                    | Some(result) -> invocation.ReturnValue <- result
                    | None -> ()
            }
        let generator = ProxyGenerator()
        generator.CreateInterfaceProxyWithoutTarget<'TContract>(realization)
