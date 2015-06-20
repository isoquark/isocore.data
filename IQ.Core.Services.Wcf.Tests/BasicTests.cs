using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IQ.Core.Services.Wcf.Tests
{
    using Wcf;
    using SampleService;

    [TestClass]
    public class BasicTests
    {
        static CustomServiceHostUtil.ConsoleServiceHostFactory _hostFactory;

        #region a few constants
        const string clientEndpointName = "TestClientEndpoint"; //must match that from the configuration file
        const string firstName = "Alpha";
        const string lastName = "Omega";
        #endregion

        [ClassInitialize]
        public static void Init(TestContext tc)
        {
            //start service host
            _hostFactory = new CustomServiceHostUtil.ConsoleServiceHostFactory();
            _hostFactory.StartRun();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            ChannelAdapter.disposeEndpointMap();
            if (_hostFactory != null) 
                _hostFactory.StopRun();
        }

        [TestMethod]
        public void TestBucketSort()
        {
            var k1 = new List<string> { "x", "a", "c", "0", "a", "b", "x", "t", "x" };
            var b = Helpers.bucketSort(k1);
            Assert.IsNotNull(b);
            Assert.IsTrue(b.First().Item1 == "0");
            var aTuple = b.ToList()[1];
            Assert.AreEqual(aTuple, new Tuple<string,int>("a", 2));
            var xCount = b.FirstOrDefault(e => e.Item1 == "x").Item2;
            Assert.AreEqual(xCount, 3);
        }

        [TestMethod]
        public void TestFindAssembly_SampleContract()
        {
            var a = ChannelAdapter.findAssemblyThatContainsContract("IQ.Core.Services.Wcf.SampleService.ISimpleService");
            Assert.IsNotNull(a.Value);
        }

        [TestMethod]
        public void TestFindAssembly_ThisTestClass()
        {
            var a = ChannelAdapter.findAssemblyThatContainsContract("IQ.Core.Services.Wcf.Tests.BasicTests");
            Assert.IsNotNull(a.Value);
        }

        [TestMethod]
        public void TestFindAssembly_InvalidTypeName()
        {
            var a = ChannelAdapter.findAssemblyThatContainsContract("This.Type.Does.Not.Exist");
            Assert.IsNull(a);
        }

        [TestMethod]
        public void TestChannelInstantion_1()
        {
            var c = ChannelAdapter.createGenericChannelInstance("IQ.Core.Services.Wcf.SampleService.ISimpleService", null);
            Assert.IsNotNull(c);
        }

        [TestMethod]
        public void TestChannelManager_SingletonInstantiation()
        {
            var cm = ChannelAdapter.ChannelManager.Instance;
            Assert.IsNotNull(cm);
            var cm1 = ChannelAdapter.ChannelManager.Instance;
            Assert.IsNotNull(cm1);
            Assert.AreSame(cm, cm1);

            var hcm = cm.GetHashCode();
            var hcm1 = cm1.GetHashCode();
            Assert.AreEqual(hcm, hcm1);
        }

        [TestMethod]
        public void Test_InvokeService_WithEndpointName_AsOption()
        {
            var cm = ChannelAdapter.ChannelManager.Instance;
            Assert.IsNotNull(cm);

            string name = null;
            
            var optStr = new Microsoft.FSharp.Core.FSharpOption<string>("TestClientEndpoint");
            cm.Invoke<ISimpleService>(x => name = x.MyRequestReplyMessage(new DataContract.Name() { FirstName = firstName, LastName = lastName }), 
                                      optStr);
            Assert.IsNotNull(name);
            Assert.IsTrue(name.ToUpper().Contains(firstName.ToUpper()));
        }

        [TestMethod]
        public void Test_InvokeService_WithEndpointName_AsString()
        {
            var cm = ChannelAdapter.ChannelManager.Instance;
            Assert.IsNotNull(cm);

            string name = null;
            cm.Invoke<ISimpleService>(x => name = x.MyRequestReplyMessage(new DataContract.Name() { FirstName = firstName, LastName = lastName }), 
                                      clientEndpointName);
            Assert.IsNotNull(name);
        }

        [TestMethod]
        public void Test_InvokeService_WithoutEndpointName()
        {
            var cm = ChannelAdapter.ChannelManager.Instance;
            Assert.IsNotNull(cm);

            string name = null;
            cm.Invoke<ISimpleService>(x => name = x.MyRequestReplyMessage(new DataContract.Name() { FirstName = firstName, LastName = lastName }));
            Assert.IsNotNull(name);
        }

        [TestMethod]
        public void Test_InvokeService_WithoutEndpointName_OneWayOp()
        {
            var cm = ChannelAdapter.ChannelManager.Instance;
            Assert.IsNotNull(cm);

            cm.Invoke<ISimpleService>(x => x.MyOneWayMessage(new Tuple<int,bool>(23,false)));
            Assert.IsTrue(true);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void Test_InvokeService_WithWrongEndpointName_AsString_FAIL()
        {
            var cm = ChannelAdapter.ChannelManager.Instance;
            Assert.IsNotNull(cm);

            string name = null;
            cm.Invoke<ISimpleService>(x => name = x.MyRequestReplyMessage(new DataContract.Name() { FirstName = firstName, LastName = lastName }),
                                      "wrongEndpointName");
        }

    }
}
