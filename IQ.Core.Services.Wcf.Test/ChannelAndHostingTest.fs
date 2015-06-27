namespace IQ.Core.Services.Wcf.Test

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

    [<Test>]
    let ``Test Singleton``() =
        let cm1 = ServiceFacade.create()
        let cm2 = ServiceFacade.create()
        cm1 |> Claim.equal cm2


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

    let cm = ServiceFacade.create()
    let cmCs = ServiceFacadeCSharp.create()

    let validateResult result  =
        Claim.isFalse <| String.IsNullOrEmpty result
        Claim.isTrue <| (result.ToUpper().Contains <| firstName.ToUpper())

    let svcOpActionCs = Action<ISimpleService> (fun (x : ISimpleService) -> result <- x.MyRequestReplyMessage(inputName))
    let svcOpFuncCs = Func<ISimpleService,string> (fun (x : ISimpleService) -> x.MyRequestReplyMessage(inputName))
    let svcOpAction : (ISimpleService -> unit) = fun (x : ISimpleService) -> result <- x.MyRequestReplyMessage(inputName)
    let svcOpFunc : (ISimpleService -> string) = fun (x : ISimpleService) -> x.MyRequestReplyMessage(inputName)
    let svcOpActionOneWay : (ISimpleService -> unit) = fun (x : ISimpleService) -> x.MyOneWayMessage(10, true) //argument values are irrelevant

    //FSharp interface tests
    [<Test>]
    let ``Invoke Service With Action and Endpoint Name``() =
        cm.Invoke (svcOpAction, epnOpt)
        validateResult result

    [<Test>]
    let ``Invoke Service With Func and Endpoint Name ``() =
        let r = cm.InvokeFun (svcOpFunc, epnOpt)
        validateResult r

    [<Test>]
    let ``Invoke Service With Action and Without Endpoint Name``() =
        cm.Invoke svcOpAction
        validateResult result

    [<Test>]
    let ``Invoke Service With Func and Without Endpoint Name``() =
        let r = cm.InvokeFun svcOpFunc
        validateResult r

    [<Test>]
    let ``Invoke OneWay Service With Action and Endpoint Name``() =
        cm.Invoke (svcOpActionOneWay, epnOpt)

    [<Test>]
    let ``Invoke OneWay Service With Action and Without Endpoint Name``() =
        cm.Invoke svcOpActionOneWay

    //CSharp interface tests
    [<Test>]
    let ``CSharp Invoke Service With Action and Endpoint Name``() =
        cmCs.Invoke (svcOpActionCs, epnStr)
        validateResult result

    [<Test>]
    let ``CSharp Invoke Service With Func and Endpoint Name``() =
        let r = cmCs.Invoke (svcOpFuncCs, epnStr) 
        validateResult r

    [<Test>]
    let ``CSharp Invoke Service With Action and Without Endpoint Name``() =
        cmCs.Invoke svcOpActionCs
        validateResult result

    [<Test>]
    let ``CSharp Invoke Service With Func and Without Endpoint Name``() =
        let r = cmCs.Invoke svcOpFuncCs
        validateResult r

    [<Test>] //TODO: how to do ExpectedException
    let ``Invoke Service With Func and Invalid Endpoint Name As String``() =
        let r = cm.InvokeFun (svcOpFunc, epnOpt) //exception here
        validateResult r



 





        