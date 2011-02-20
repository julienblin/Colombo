#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.ServiceModel;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Colombo.Facilities;
using Colombo.Wcf;
using NUnit.Framework;

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

            using (var serviceHostServer = new ServiceHost(typeof(ColomboService), new Uri(@"net.pipe://localhost/ipctest")))
            {
                serviceHostServer.Open();

                var request1 = new TestRequestIPC { Name = "Request1" };
                var request2 = new TestRequestIPC { Name = "Request2" };

                var messageBus = clientContainer.Resolve<IMessageBus>();
                var responses = messageBus.Send(request1, request2);
                var response1 = responses.GetFrom(request1);
                var response2 = responses.GetFrom(request2);

                Assert.That(response1.Name, Is.EqualTo(request1.Name));
                Assert.That(response1.CorrelationGuid, Is.EqualTo(request1.CorrelationGuid));

                Assert.That(response2.Name, Is.EqualTo(request2.Name));
                Assert.That(response2.CorrelationGuid, Is.EqualTo(request2.CorrelationGuid));
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

            using (var serviceHostServer = new ServiceHost(typeof(ColomboService), new Uri(@"net.pipe://localhost/ipctest")))
            {
                serviceHostServer.Open();

                var request1 = new TestRequestIPC { Name = "Request1" };
                var request2 = new TestRequestIPC { Name = "Request2" };

                var messageBus = clientContainer.Resolve<IStatefulMessageBus>();
                var response1 = messageBus.FutureSend(request1);
                var response2 = messageBus.FutureSend(request2);

                Assert.That(response1.Name, Is.EqualTo(request1.Name));
                Assert.That(response1.CorrelationGuid, Is.EqualTo(request1.CorrelationGuid));

                Assert.That(response2.Name, Is.EqualTo(request2.Name));
                Assert.That(response2.CorrelationGuid, Is.EqualTo(request2.CorrelationGuid));
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

            using (var serviceHostServer = new ServiceHost(typeof(ColomboService), new Uri(@"net.pipe://localhost/ipctest")))
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

                Assert.That(response1.Name, Is.EqualTo(request1.Name));
                Assert.That(response1.CorrelationGuid, Is.EqualTo(request1.CorrelationGuid));

                Assert.That(response2.Name, Is.EqualTo(request2.Name));
                Assert.That(response2.CorrelationGuid, Is.EqualTo(request2.CorrelationGuid));

                Assert.That(response3.Name, Is.EqualTo(request3.Name));
                Assert.That(response3.CorrelationGuid, Is.EqualTo(request3.CorrelationGuid));

                Assert.That(response4.Name, Is.EqualTo(request4.Name));
                Assert.That(response4.CorrelationGuid, Is.EqualTo(request4.CorrelationGuid));
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
            protected override void Handle()
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
            protected override void Handle()
            {
                Response.Name = Request.Name;
            }
        }
    }
}
