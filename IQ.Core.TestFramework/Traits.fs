namespace IQ.Core.TestFramework

open System
open System.IO
open System.Diagnostics
open System.Collections.Generic
open System.Linq
open System.Reflection

open Xunit
open Xunit.Sdk
open Xunit.Abstractions

open IQ.Core.Data
open IQ.Core.Framework

//See: https://github.com/xunit/samples.xunit/blob/master/TraitExtensibility/

/// <summary>
/// Applied to a test to asign it to a category
/// </summary>
type TraitAttribute = Xunit.TraitAttribute


/// <summary>
/// This class discovers all of the tests and test classes that have
/// applied the Category attribute
/// </summary>
type CategoryDiscoverer() =
    interface ITraitDiscoverer with
        member this.GetTraits(attrib) =
            seq{
                let args = attrib.GetConstructorArguments().ToList()
                yield KeyValuePair("Category", args.[0].ToString())
                }
        
/// <summary>
/// Apply this attribute to your test method to specify a category.
/// </summary>
[<TraitDiscoverer("IQ.Core.TestFramework.CategoryDiscoverer", AssemblyLiterals.ShortAssemblyName)>]
[<AttributeUsage(AttributeTargets.All, AllowMultiple = true)>]
type CategoryAttribute(category) =
    inherit Attribute()

    member this.Category : string = category            

    with interface ITraitAttribute end


/// <summary>
/// This class discovers all of the tests and test classes to which the BenchmarkTrait
/// attribute has been applied
/// </summary>
type BenchmarkDiscoverer() =
    interface ITraitDiscoverer with
        member this.GetTraits(attrib) =
            seq{
                let args = attrib.GetConstructorArguments().ToList()
                //yield KeyValuePair("Benchmarks", args.[0].ToString())                
                yield KeyValuePair("Category", "Benchmarks")
                }
        
/// <summary>
/// Apply this attribute to your test method to specify a category.
/// </summary>
[<TraitDiscoverer("IQ.Core.TestFramework.BenchmarkDiscoverer", AssemblyLiterals.ShortAssemblyName)>]
[<AttributeUsage(AttributeTargets.All, AllowMultiple = true)>]
type BenchmarkAttribute(opcount) =
    inherit Attribute()

    new () =
        BenchmarkAttribute(0)
    
    member this.OperationCount : int = opcount

    with interface ITraitAttribute end


