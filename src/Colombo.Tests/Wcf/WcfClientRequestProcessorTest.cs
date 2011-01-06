using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Colombo.Wcf;
using System.ServiceModel;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class WcfClientRequestProcessorTest : BaseTest
    {
        [Test]
        public void It_should_use_IWcfClientBaseServiceFactory_to_determine_if_it_can_process()
        {
            var mocks = new MockRepository();
            var requestInThisAssembly = new TestRequest();
            var requestInDynamicAssembly = mocks.Stub<Request<TestResponse>>();
            var clientBaseFactory = mocks.StrictMock<IWcfClientBaseServiceFactory>();

            With.Mocks(mocks).Expecting(() =>
            {
                requestInDynamicAssembly.GetGroupName();
                LastCall.Do(new GetGroupNameDelegate(() =>
                {
                    return "somethingelse";
                }));
                Expect.Call(clientBaseFactory.CanCreateClientBaseForRequestGroup(requestInThisAssembly.GetGroupName()))
                    .Return(true);
                Expect.Call(clientBaseFactory.CanCreateClientBaseForRequestGroup("somethingelse"))
                    .Return(false);
            }).Verify(() =>
            {
                var processor = new WcfClientRequestProcessor(clientBaseFactory);
                processor.Logger = GetConsoleLogger();

                Assert.That(() => processor.CanProcess(requestInThisAssembly),
                    Is.True);
                Assert.That(() => processor.CanProcess(requestInDynamicAssembly),
                    Is.False);
            });
        }

        public delegate string GetGroupNameDelegate();

        [Test]
        public void It_should_use_WCF_to_process_requests()
        {
            const string IPCAddress1 = @"net.pipe://localhost/ipctest1";
            const string IPCAddress2 = @"net.pipe://localhost/ipctest2";
            // Create named-pipe services host for testing...
            using (ServiceHost serviceHost1 = new ServiceHost(typeof(TestWcfService1), new Uri(IPCAddress1)))
            using (ServiceHost serviceHost2 = new ServiceHost(typeof(TestWcfService2), new Uri(IPCAddress2)))
            {
                serviceHost1.Open();
                serviceHost2.Open();

                var mocks = new MockRepository();

                var clientBaseFactory = mocks.StrictMock<IWcfClientBaseServiceFactory>();

                var request1 = new TestRequestIPC1 { Name = @"Request1" };
                var request2 = new TestRequestIPC2 { Name = @"Request2" };
                var request3 = new TestRequestIPC1 { Name = @"Request3" };
                var request4 = new TestRequestIPC2 { Name = @"Request4" };
                var requests = new List<BaseRequest> { request1, request2, request3, request4 };

                With.Mocks(mocks).Expecting(() =>
                {
                    Expect.Call(clientBaseFactory.CanCreateClientBaseForRequestGroup(request1.GetGroupName())).Return(true);
                    Expect.Call(clientBaseFactory.CanCreateClientBaseForRequestGroup(request2.GetGroupName())).Return(true);

                    Expect.Call(clientBaseFactory.GetAddressForRequestGroup(request1.GetGroupName())).Return(IPCAddress1);
                    Expect.Call(clientBaseFactory.GetAddressForRequestGroup(request2.GetGroupName())).Return(IPCAddress2);

                    Expect.Call(clientBaseFactory.CreateClientBase(request1.GetGroupName())).Do(new CreateClientBaseDelegate((name) =>
                    {
                        return new WcfClientBaseService(new NetNamedPipeBinding(), new EndpointAddress(IPCAddress1));
                    }));
                    Expect.Call(clientBaseFactory.CreateClientBase(request2.GetGroupName())).Do(new CreateClientBaseDelegate((name) =>
                    {
                        return new WcfClientBaseService(new NetNamedPipeBinding(), new EndpointAddress(IPCAddress2));
                    }));
                }).Verify(() =>
                {
                    var processor = new WcfClientRequestProcessor(clientBaseFactory);
                    processor.Logger = GetConsoleLogger();

                    var responses = processor.Process(requests);

                    Assert.That(responses.GetFrom(request1).Name,
                        Is.EqualTo(request1.Name));
                    Assert.That(responses.GetFrom(request2).Name,
                        Is.EqualTo(request2.Name));
                    Assert.That(responses.GetFrom(request3).Name,
                        Is.EqualTo(request3.Name));
                    Assert.That(responses.GetFrom(request4).Name,
                        Is.EqualTo(request4.Name));
                });

                serviceHost1.Abort();
                serviceHost2.Abort();
            }
        }

        public delegate WcfClientBaseService CreateClientBaseDelegate(string name);

        public class TestRequest : Request<TestResponse>
        {
        }

        public class TestResponseIpc : Response
        {
            public string Name { get; set; }
        }

        public class TestRequestIPC1 : SideEffectFreeRequest<TestResponseIpc>
        {
            public string Name { get; set; }

            public override string GetGroupName()
            {
                return "ipctest1";
            }
        }

        public class TestWcfService1 : IWcfService
        {
            public Response[] Process(BaseRequest[] requests)
            {
                return new Response[] { 
                    new TestResponseIpc { Name = ((TestRequestIPC1)requests[0]).Name },
                    new TestResponseIpc { Name = ((TestRequestIPC1)requests[1]).Name }
                };
            }
        }

        public class TestRequestIPC2 : SideEffectFreeRequest<TestResponseIpc>
        {
            public string Name { get; set; }

            public override string GetGroupName()
            {
                return "ipctest2";
            }
        }

        public class TestWcfService2 : IWcfService
        {
            public Response[] Process(BaseRequest[] requests)
            {
                return new Response[] { 
                    new TestResponseIpc { Name = ((TestRequestIPC2)requests[0]).Name },
                    new TestResponseIpc { Name = ((TestRequestIPC2)requests[1]).Name }
                };
            }
        }

    }
}
