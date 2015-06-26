﻿namespace IQ.Core.Services.Wcf.Test

open System
open IQ.Core.Framework
open IQ.Core.TestFramework
open IQ.Core.Services.Wcf

open IQ.Core.Services.Wcf.SampleService

[<TestContainer>]
module ``No-Host No-Channel Helper Tests`` =

    [<Test>]
    let ``Bucket Sort Success Test``() =
        let k1 = [ "x"; "a"; "c"; "0"; "a"; "b"; "x"; "t"; "x" ]
        let b = Helpers.bucketSort(k1);
        Claim.isNotNull b
        Claim.isTrue (b |> Seq.count > 0)

        let actual = k1 |> Seq.sort |> Seq.head
        let expect = b |> Seq.head |> fst
        actual |> Claim.equal expect

        let groupingByKey = k1 |> Seq.groupBy (fun x -> x)
        let xGroup = groupingByKey |> Seq.tryFind (fun (g, _) -> g = "x")

        let expect = Some ("x",3)
        let actual = b |> Seq.tryFind (fun (n,_) -> n = "x")
        Claim.isSome actual
        actual |> Claim.equal expect


[<TestContainer>]
module ``Channel and Service Hosting Tests`` =

    let clientEndpointName = "TestClientEndpoint" //must match that from the configuration file
    let firstName = "Alpha"
    let lastName = "Omega"
    let epnStr = "TestClientEndpoint"
    let epnOpt = Some epnStr
    let mutable result = ""
    let _hostFactory : CustomServiceHostUtil.ConsoleServiceHostFactory = new CustomServiceHostUtil.ConsoleServiceHostFactory()

    [<TestInit>]
    let Init() =
        _hostFactory.StartRun()

    [<TestCleanup>]
    let Cleanup() =
        //ChannelAdapter.disposeEndpointMap() //cannot dispose after each test because it is a singleton
        _hostFactory.StopRun()

    let inputName = 
        let nameObj = new DataContract.Name()
        nameObj.FirstName <- firstName
        nameObj.LastName <- lastName
        nameObj

    let cm = ChannelAdapter.ChannelManager.Instance
    let validateResult result  =
        Claim.isFalse <| String.IsNullOrEmpty result
        Claim.isTrue <| (result.ToUpper().Contains <| firstName.ToUpper())

    let actionAsAction() = Action<ISimpleService> (fun (x : ISimpleService) -> result <- x.MyRequestReplyMessage(inputName))
    let actionAsFun = fun (x : 'T when 'T :> ISimpleService) -> result <- x.MyRequestReplyMessage(inputName)

    [<Test>]
    let ``Invoke Service With Action and Endpoint Name As Option``() =
        cm.Invoke (actionAsAction(), epnOpt)
        validateResult result

    [<Test>]
    let ``Invoke Service With Func and Endpoint Name As Option``() =
        cm.Invoke2 (actionAsFun, epnOpt)
        validateResult result

    [<Test>]
    let ``Invoke Service With Action and Endpoint Name As String``() =
        cm.Invoke (actionAsAction(), epnStr)
        validateResult result

    [<Test>]
    let ``Invoke Service With Func and Endpoint Name As String``() =
        cm.Invoke2 (actionAsFun, epnStr)
        validateResult result

    [<Test>]
    let ``Invoke Service With Action and Without Endpoint Name``() =
        cm.Invoke (actionAsAction())
        validateResult result

    [<Test>]
    let ``Invoke Service With Func and Without Endpoint Name``() =
        cm.Invoke2 (actionAsFun)
        validateResult result

//    [<Test>] //TODO: how to do ExpectedException
//    let ``Invoke Service With Func and Invalid Endpoint Name As String``() =
//        cm.Invoke2 (actionAsFun, epnStr) //exception here
//        validateResult result



 





        