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



[<AutoOpen>]
module TestVocabulary =
    /// <summary>
    /// Identifies a test method
    /// </summary>
    type FactAttribute = Xunit.FactAttribute

    /// <summary>
    /// Applied to a test to asign it to a category
    /// </summary>
    type TraitAttribute = Xunit.TraitAttribute

    //See http://xunit.github.io/docs/shared-context.html
    [<AbstractClass>]
    type TestCollection<'T  when 'T:(new : unit->'T) and 'T: not struct>() =
        interface ICollectionFixture<'T> 
    
    type TestCollectionMakerAttribute = CollectionDefinitionAttribute
    
    type TestContainerAttribute = CollectionAttribute
        
    type Assert = Xunit.Assert
    
    type ITestLog = Xunit.Abstractions.ITestOutputHelper


        
                                 
