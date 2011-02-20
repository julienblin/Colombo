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
using Colombo.Alerts;
using Colombo.Wcf;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class WcfClientRequestProcessorTest : BaseTest
    {
        [Test]
        public void It_should_use_IWcfServiceFactory_to_determine_if_it_can_process()
        {
            var mocks = new MockRepository();
            var requestInThisAssembly = new TestRequest();
            var requestInDynamicAssembly = mocks.Stub<Request<TestResponse>>();
            var wcfServiceFactory = mocks.StrictMock<IColomboServiceFactory>();

            With.Mocks(mocks).Expecting(() =>
            {
                requestInDynamicAssembly.GetGroupName();
                LastCall.Do(new GetGroupNameDelegate(() =>
                {
                    return "somethingelse";
                }));
                Expect.Call(wcfServiceFactory.CanCreateChannelForRequestGroup(requestInThisAssembly.GetGroupName()))
                    .Return(true);
                Expect.Call(wcfServiceFactory.CanCreateChannelForRequestGroup("somethingelse"))
                    .Return(false);
            }).Verify(() =>
            {
                var processor = new WcfClientRequestProcessor(wcfServiceFactory);
                processor.Logger = GetConsoleLogger();

                Assert.That(processor.CanProcess(requestInThisAssembly), Is.True);
                Assert.That(processor.CanProcess(requestInDynamicAssembly), Is.False);
            });
        }

        public delegate string GetGroupNameDelegate();

        [Test]
        public void It_should_use_WCF_to_process_requests()
        {
            const string IPCAddress1 = @"net.pipe://localhost/ipctest1";
            const string IPCAddress2 = @"net.pipe://localhost/ipctest2";
            // Create named-pipe services host for testing...
            using (var serviceHost1 = new ServiceHost(typeof(TestService1), new Uri(IPCAddress1)))
            using (var serviceHost2 = new ServiceHost(typeof(TestService2), new Uri(IPCAddress2)))
            {
                serviceHost1.Open();
                serviceHost2.Open();

                var mocks = new MockRepository();

                var wcfServiceFactory = mocks.StrictMock<IColomboServiceFactory>();

                var request1 = new TestRequestIPC1 { Name = @"Request1" };
                var request2 = new TestRequestIPC2 { Name = @"Request2" };
                var request3 = new TestRequestIPC1 { Name = @"Request3" };
                var request4 = new TestRequestIPC2 { Name = @"Request4" };
                var requests = new List<BaseRequest> { request1, request2, request3, request4 };

                var channelFactory1 = new ChannelFactory<IColomboService>(new NetNamedPipeBinding(), new EndpointAddress(IPCAddress1));
                var channelFactory2 = new ChannelFactory<IColomboService>(new NetNamedPipeBinding(), new EndpointAddress(IPCAddress2));

                With.Mocks(mocks).Expecting(() =>
                {
                    Expect.Call(wcfServiceFactory.CanCreateChannelForRequestGroup(request1.GetGroupName())).Return(true);
                    Expect.Call(wcfServiceFactory.CanCreateChannelForRequestGroup(request2.GetGroupName())).Return(true);

                    Expect.Call(wcfServiceFactory.GetAddressForRequestGroup(request1.GetGroupName())).Return(IPCAddress1);
                    Expect.Call(wcfServiceFactory.GetAddressForRequestGroup(request2.GetGroupName())).Return(IPCAddress2);

                    Expect.Call(wcfServiceFactory.CreateChannel(request1.GetGroupName())).Do(new CreateChannelDelegate((name) =>
                    {
                        return channelFactory1.CreateChannel();
                    }));
                    Expect.Call(wcfServiceFactory.CreateChannel(request2.GetGroupName())).Do(new CreateChannelDelegate((name) =>
                    {
                        return channelFactory2.CreateChannel();
                    }));
                }).Verify(() =>
                {
                    var processor = new WcfClientRequestProcessor(wcfServiceFactory);
                    processor.Logger = GetConsoleLogger();

                    var responses = processor.Process(requests);

                    Assert.That(responses.GetFrom(request1).Name, Is.EqualTo(request1.Name));
                    Assert.That(responses.GetFrom(request2).Name, Is.EqualTo(request2.Name));
                    Assert.That(responses.GetFrom(request3).Name, Is.EqualTo(request3.Name));
                    Assert.That(responses.GetFrom(request4).Name, Is.EqualTo(request4.Name));
                });
            }
        }

        [Test]
        public void It_should_emit_HealthCheck_alerts_if_endpoint_failed()
        {
            var mocks = new MockRepository();

            var wcfServiceFactory = mocks.StrictMock<IColomboServiceFactory>();
            var alerter = mocks.StrictMock<IColomboAlerter>();

            var channelFactory = new ChannelFactory<IColomboService>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/unknown"));

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(wcfServiceFactory.CreateChannelsForAllEndPoints()).Return(new[] { channelFactory.CreateChannel() });
                alerter.Alert(null);
                LastCall.IgnoreArguments().Constraints(
                    Rhino.Mocks.Constraints.Is.TypeOf(typeof(HealthCheckFailedAlert))
                );
            }).Verify(() =>
            {
                var processor = new WcfClientRequestProcessor(wcfServiceFactory);
                processor.Logger = GetConsoleLogger();
                processor.Alerters = new[] { alerter };

                processor.HealthCheckTimerElapsed(null, null);
            });
        }

        public delegate IColomboService CreateChannelDelegate(string name);

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

        [ServiceBehavior(
            IncludeExceptionDetailInFaults = true,
            ConcurrencyMode = ConcurrencyMode.Multiple,
            InstanceContextMode = InstanceContextMode.PerCall
        )]
        public class TestService1 : IColomboService
        {
            public IAsyncResult BeginProcessAsync(BaseRequest[] requests, AsyncCallback callback, object state)
            {
                var asyncResult = new ProcessAsyncResult(callback, state);
                asyncResult.Responses = new Response[] { 
                    new TestResponseIpc { Name = ((TestRequestIPC1)requests[0]).Name },
                    new TestResponseIpc { Name = ((TestRequestIPC1)requests[1]).Name }
                };
                asyncResult.OnCompleted();
                return asyncResult;
            }

            public Response[] EndProcessAsync(IAsyncResult asyncResult)
            {
                using (var processResult = asyncResult as ProcessAsyncResult)
                {
                    processResult.AsyncWaitHandle.WaitOne();
                    return processResult.Responses;
                }
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

        [ServiceBehavior(
            IncludeExceptionDetailInFaults = true,
            ConcurrencyMode = ConcurrencyMode.Multiple,
            InstanceContextMode = InstanceContextMode.PerCall
        )]
        public class TestService2 : IColomboService
        {
            public IAsyncResult BeginProcessAsync(BaseRequest[] requests, AsyncCallback callback, object state)
            {
                var asyncResult = new ProcessAsyncResult(callback, state);
                asyncResult.Responses = new Response[] { 
                    new TestResponseIpc { Name = ((TestRequestIPC2)requests[0]).Name },
                    new TestResponseIpc { Name = ((TestRequestIPC2)requests[1]).Name }
                };
                asyncResult.OnCompleted();
                return asyncResult;
            }

            public Response[] EndProcessAsync(IAsyncResult asyncResult)
            {
                using (var processResult = asyncResult as ProcessAsyncResult)
                {
                    processResult.AsyncWaitHandle.WaitOne();
                    return processResult.Responses;
                }
            }
        }

    }
}
