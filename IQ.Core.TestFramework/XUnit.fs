namespace IQ.Core.TestFramework

open System
open System.IO
open System.Diagnostics

open Xunit

module XUnit =
    /// <summary>
    /// Identifies a test method
    /// </summary>
    type FactAttribute = Xunit.FactAttribute

    //See http://xunit.github.io/docs/shared-context.html
    [<AbstractClass>]
    type TestCollection<'T  when 'T:(new : unit->'T) and 'T: not struct>() =
        interface ICollectionFixture<'T> 
    
    type TestCollectionMakerAttribute = CollectionDefinitionAttribute
    
    type TestContainerAttribute = CollectionAttribute
    
    
    type Assert = Xunit.Assert
    
    type ITestLog = Xunit.Abstractions.ITestOutputHelper

    [<AbstractClass>]
    type TestContext() = class end

    [<AbstractClass>]
    type TestContainer<'T>(log) =
        member this.Log : ITestLog = log
        
    type TraitAttribute = Xunit.TraitAttribute
