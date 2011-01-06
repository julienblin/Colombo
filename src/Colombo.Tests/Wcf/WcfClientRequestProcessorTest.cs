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
        public void It_should_check_WCF_client_configuration_to_determine_if_it_can_process()
        {
            var mocks = new MockRepository();

            var processor = new WcfClientRequestProcessor();
            processor.Logger = GetConsoleLogger();
            var requestInThisAssembly = new TestRequest();
            var requestInDynamicAssembly = mocks.Stub<Request<TestResponse>>();

            Assert.That(() => processor.CanProcess(requestInThisAssembly),
                Is.True);
            Assert.That(() => processor.CanProcess(requestInDynamicAssembly),
                Is.False);
        }

        [Test]
        public void It_should_create_a_ClientBase_from_configuration()
        {
            var processor = new WcfClientRequestProcessor();
            processor.Logger = GetConsoleLogger();

            var clientBase = processor.CreateClientBase("Colombo.Tests");

            Assert.That(() => clientBase.Endpoint.Address.Uri,
                Is.EqualTo(new Uri(@"http://localhost/Colombo.svc")));
        }

        [Test]
        public void It_should_use_WCF_to_process_requests()
        {
            // Create named-pipe services host for testing...
            using (ServiceHost serviceHost1 = new ServiceHost(typeof(TestWcfService1), new Uri("net.pipe://localhost/ipctest1")))
            using (ServiceHost serviceHost2 = new ServiceHost(typeof(TestWcfService2), new Uri("net.pipe://localhost/ipctest2")))
            {
                serviceHost1.Open();
                serviceHost2.Open();

                var processor = new WcfClientRequestProcessor();
                processor.Logger = GetConsoleLogger();

                var request1 = new TestRequestIPC1 { Name = @"Request1" };
                var request2 = new TestRequestIPC2 { Name = @"Request2" };
                var request3 = new TestRequestIPC1 { Name = @"Request3" };
                var request4 = new TestRequestIPC2 { Name = @"Request4" };
                var requests = new List<BaseRequest> { request1, request2, request3, request4 };

                var responses = processor.Process(requests);

                Assert.That(responses.GetFrom(request1).Name,
                    Is.EqualTo(request1.Name));
                Assert.That(responses.GetFrom(request2).Name,
                    Is.EqualTo(request2.Name));
                Assert.That(responses.GetFrom(request3).Name,
                    Is.EqualTo(request3.Name));
                Assert.That(responses.GetFrom(request4).Name,
                    Is.EqualTo(request4.Name));
            }
        }

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
