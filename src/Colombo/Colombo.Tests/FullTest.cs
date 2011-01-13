using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Castle.Windsor;
using Colombo.Facilities;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using System.ServiceModel;
using Colombo.Wcf;

namespace Colombo.Tests
{
    [TestFixture]
    public class FullTest
    {
        [Test]
        public void It_should_work_with_WCF_client_and_server_ClientOnly()
        {
            // Setup client
            var clientContainer = new WindsorContainer();
            clientContainer.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Console));
            clientContainer.AddFacility<ColomboFacility>(f => f.ClientOnly());

            // Setup server
            var serverContainer = new WindsorContainer();
            serverContainer.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Console));
            serverContainer.AddFacility<ColomboFacility>();
            serverContainer.Register(
                Component.For<ISideEffectFreeRequestHandler<TestRequestIPC, TestResponseIpc>>()
                    .LifeStyle.Transient
                    .ImplementedBy<TestRequestIPCHandler>()
            );

            using (ServiceHost serviceHostServer = new ServiceHost(typeof(WcfColomboService), new Uri(@"net.pipe://localhost/ipctest")))
            {
                serviceHostServer.Open();

                var request1 = new TestRequestIPC { Name = "Request1" };
                var request2 = new TestRequestIPC { Name = "Request2" };

                var messageBus = clientContainer.Resolve<IMessageBus>();
                var responses = messageBus.Send(request1, request2);
                var response1 = responses.GetFrom(request1);
                var response2 = responses.GetFrom(request2);

                Assert.That(() => response1.Name,
                    Is.EqualTo(request1.Name));
                Assert.That(() => response1.CorrelationGuid,
                    Is.EqualTo(request1.CorrelationGuid));

                Assert.That(() => response2.Name,
                    Is.EqualTo(request2.Name));
                Assert.That(() => response2.CorrelationGuid,
                    Is.EqualTo(request2.CorrelationGuid));
            }
        }

        [Test]
        public void It_should_work_with_WCF_client_server_ClientOnly_and_IStateFulMessageBus()
        {
            // Setup client
            var clientContainer = new WindsorContainer();
            clientContainer.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Console));
            clientContainer.AddFacility<ColomboFacility>(f => f.ClientOnly());

            // Setup server
            var serverContainer = new WindsorContainer();
            serverContainer.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Console));
            serverContainer.AddFacility<ColomboFacility>();
            serverContainer.Register(
                Component.For<ISideEffectFreeRequestHandler<TestRequestIPC, TestResponseIpc>>()
                    .LifeStyle.Transient
                    .ImplementedBy<TestRequestIPCHandler>()
            );

            using (ServiceHost serviceHostServer = new ServiceHost(typeof(WcfColomboService), new Uri(@"net.pipe://localhost/ipctest")))
            {
                serviceHostServer.Open();

                var request1 = new TestRequestIPC { Name = "Request1" };
                var request2 = new TestRequestIPC { Name = "Request2" };

                var messageBus = clientContainer.Resolve<IStatefulMessageBus>();
                var response1 = messageBus.FutureSend(request1);
                var response2 = messageBus.FutureSend(request2);

                Assert.That(() => response1.Name,
                    Is.EqualTo(request1.Name));
                Assert.That(() => response1.CorrelationGuid,
                    Is.EqualTo(request1.CorrelationGuid));

                Assert.That(() => response2.Name,
                    Is.EqualTo(request2.Name));
                Assert.That(() => response2.CorrelationGuid,
                    Is.EqualTo(request2.CorrelationGuid));
            }
        }

        [Test]
        public void It_should_work_with_WCF_client_and_server()
        {
            // Setup client
            var clientContainer = new WindsorContainer();
            clientContainer.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Console));
            clientContainer.AddFacility<ColomboFacility>();
            clientContainer.Register(
                Component.For<ISideEffectFreeRequestHandler<TestRequestLocal, TestResponseLocal>>()
                    .LifeStyle.Transient
                    .ImplementedBy<TestRequestHandlerLocal>()
            );

            // Setup server
            var serverContainer = new WindsorContainer();
            serverContainer.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Console));
            serverContainer.AddFacility<ColomboFacility>();
            serverContainer.Register(
                Component.For<ISideEffectFreeRequestHandler<TestRequestIPC, TestResponseIpc>>()
                    .LifeStyle.Transient
                    .ImplementedBy<TestRequestIPCHandler>()
            );

            using (ServiceHost serviceHostServer = new ServiceHost(typeof(WcfColomboService), new Uri(@"net.pipe://localhost/ipctest")))
            {
                serviceHostServer.Open();
                
                var request1 = new TestRequestIPC { Name = "Request1" };
                var request2 = new TestRequestLocal { Name = "Request2" };
                var request3 = new TestRequestLocal { Name = "Request2" };
                var request4 = new TestRequestIPC { Name = "Request2" };

                var messageBus = clientContainer.Resolve<IMessageBus>();
                var responses = messageBus.Send(request1, request2, request3, request4);
                var response1 = responses.GetFrom(request1);
                var response2 = responses.GetFrom(request2);
                var response3 = responses.GetFrom(request3);
                var response4 = responses.GetFrom(request4);

                Assert.That(() => response1.Name,
                    Is.EqualTo(request1.Name));
                Assert.That(() => response1.CorrelationGuid,
                    Is.EqualTo(request1.CorrelationGuid));

                Assert.That(() => response2.Name,
                    Is.EqualTo(request2.Name));
                Assert.That(() => response2.CorrelationGuid,
                    Is.EqualTo(request2.CorrelationGuid));

                Assert.That(() => response3.Name,
                    Is.EqualTo(request3.Name));
                Assert.That(() => response3.CorrelationGuid,
                    Is.EqualTo(request3.CorrelationGuid));

                Assert.That(() => response4.Name,
                    Is.EqualTo(request4.Name));
                Assert.That(() => response4.CorrelationGuid,
                    Is.EqualTo(request4.CorrelationGuid));
            }
        }

        public class TestResponseIpc : Response
        {
            public virtual string Name { get; set; }
        }

        public class TestRequestIPC : SideEffectFreeRequest<TestResponseIpc>
        {
            public string Name { get; set; }

            public override string GetGroupName()
            {
                return "ipctest";
            }
        }

        public class TestRequestIPCHandler : SideEffectFreeRequestHandler<TestRequestIPC, TestResponseIpc>
        {
            public override void Handle()
            {
                Response.Name = Request.Name;
            }
        }

        public class TestResponseLocal : Response
        {
            public virtual string Name { get; set; }
        }

        public class TestRequestLocal : SideEffectFreeRequest<TestResponseLocal>
        {
            public string Name { get; set; }

            public override string GetGroupName()
            {
                return "local";
            }
        }

        public class TestRequestHandlerLocal : SideEffectFreeRequestHandler<TestRequestLocal, TestResponseLocal>
        {
            public override void Handle()
            {
                Response.Name = Request.Name;
            }
        }
    }
}
