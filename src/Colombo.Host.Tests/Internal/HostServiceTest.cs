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
using System.Collections.Generic;
using System.ServiceModel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Colombo.Host.Internal;
using NUnit.Framework;

namespace Colombo.Host.Tests.Internal
{
    [TestFixture]
    public class HostServiceTest
    {
        [Test]
        public void It_should_call_all_the_defined_interfaces_in_endpoint()
        {
            var hostService = new HostService();
            hostService.ConfigureThisEndpointType = typeof (TestEndPoint);
            hostService.Start();
            hostService.Stop();

            var testEndPoint = (TestEndPoint) hostService.ConfigureThisEndpoint;

            Assert.That(testEndPoint.CreateContainerCalled);
            Assert.That(testEndPoint.ConfigureLoggingCalled);
            Assert.That(testEndPoint.ConfigureColomboCalled);
            Assert.That(testEndPoint.RegisterMessageHandlersCalled);
            Assert.That(testEndPoint.RegisterOtherComponentsCalled);
            Assert.That(testEndPoint.CreateServiceHostsCalled);
            Assert.That(testEndPoint.StartCalled);
            Assert.That(testEndPoint.StopCalled);
        }

        [Test]
        public void It_should_throw_an_exception_when_create_container_returns_null()
        {
            var hostService = new HostService();
            hostService.ConfigureThisEndpointType = typeof(TestEndPointBadContainer);

            Assert.That(() => hostService.Start(),
                       Throws.Exception.TypeOf<ColomboHostException>()
                           .With.Message.Contains("container"));
        }

        public class TestEndPoint : IAmAnEndpoint,
                                    IWantToCreateTheContainer, IWantToConfigureLogging, IWantToConfigureColombo,
                                    IWantToRegisterMessageHandlers, IWantToRegisterOtherComponents, IWantToCreateServiceHosts,
                                    IWantToBeNotifiedWhenStartAndStop
        {
            public bool CreateContainerCalled { get; private set; }
            public bool ConfigureLoggingCalled { get; private set; }
            public bool ConfigureColomboCalled { get; private set; }
            public bool RegisterMessageHandlersCalled { get; private set; }
            public bool RegisterOtherComponentsCalled { get; private set; }
            public bool CreateServiceHostsCalled { get; private set; }
            public bool StartCalled { get; private set; }
            public bool StopCalled { get; private set; }

            public IWindsorContainer CreateContainer()
            {
                CreateContainerCalled = true;
                return new WindsorContainer();
            }

            public void ConfigureLogging(IWindsorContainer container)
            {
                ConfigureLoggingCalled = true;
            }

            public void ConfigureColombo(IWindsorContainer container)
            {
                ConfigureColomboCalled = true;
                container.Register(Component.For<IMessageBus>().ImplementedBy<FakeMessageBus>());
            }

            public void RegisterMessageHandlers(IWindsorContainer container)
            {
                RegisterMessageHandlersCalled = true;
            }

            public void RegisterOtherComponents(IWindsorContainer container)
            {
                RegisterOtherComponentsCalled = true;
            }

            public IEnumerable<ServiceHost> CreateServiceHosts(IWindsorContainer container)
            {
                CreateServiceHostsCalled = true;
                yield break;
            }

            public void Start(IWindsorContainer container)
            {
                StartCalled = true;
            }

            public void Stop(IWindsorContainer container)
            {
                StopCalled = true;
            }
        }

        public class TestEndPointBadContainer : IAmAnEndpoint,
                                                IWantToCreateTheContainer
        {
            public IWindsorContainer CreateContainer()
            {
                return null;
            }
        }

        public class FakeMessageBus : IMessageBus
        {
            public Response Send(BaseRequest request)
            {
                throw new NotImplementedException();
            }

            public TResponse Send<TResponse>(Request<TResponse> request) where TResponse : Response, new()
            {
                throw new NotImplementedException();
            }

            public IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request) where TResponse : Response, new()
            {
                throw new NotImplementedException();
            }

            public TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request) where TResponse : Response, new()
            {
                throw new NotImplementedException();
            }

            public TResponse Send<TRequest, TResponse>(Action<TRequest> action)
                where TRequest : SideEffectFreeRequest<TResponse>, new()
                where TResponse : Response, new()
            {
                throw new NotImplementedException();
            }

            public IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request) where TResponse : Response, new()
            {
                throw new NotImplementedException();
            }

            public ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
            {
                throw new NotImplementedException();
            }

            public void Notify(Notification notification, params Notification[] notifications)
            {
                throw new NotImplementedException();
            }
        }

    }
}
