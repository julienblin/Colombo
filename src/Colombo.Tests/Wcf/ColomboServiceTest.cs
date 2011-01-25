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
using System.Threading;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Colombo.Wcf;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class ColomboServiceTest : BaseTest
    {
        [SetUp]
        public void SetUp()
        {
            WcfServices.Kernel = null;
        }

        [Test]
        public void It_should_raise_an_exception_when_processing_with_a_null_kernel()
        {
            var mocks = new MockRepository();
            var request1 = mocks.Stub<Request<TestResponse>>();
            var request2 = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request1, request2 };
            var service = new ColomboService();

            Exception testedException = null;

            var asyncResult = service.BeginProcessAsync(requests, (ar) =>
            {
                try { service.EndProcessAsync(ar); }
                catch (Exception ex) { testedException = ex; }
            }, null);

            Thread.Sleep(200);
            while (testedException == null) ;

            Assert.That(() => testedException,
                Is.TypeOf<ColomboException>()
                .With.Message.Contains("Kernel"));
        }

        [Test]
        public void It_should_throw_an_exception_when_not_finding_a_ILocalRequestProcessor()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request };
            var service = new ColomboService();

            var kernel = new DefaultKernel();

            WcfServices.Kernel = kernel;

            Exception testedException = null;

            var asyncResult = service.BeginProcessAsync(requests, (ar) =>
            {
                try { service.EndProcessAsync(ar); }
                catch (Exception ex) { testedException = ex; }
            }, null);

            Thread.Sleep(200);
            while (testedException == null) ;

            Assert.That(() => testedException,
                Is.TypeOf<ColomboException>()
                .With.Message.Contains("ILocalMessageProcessor"));
        }

        [Test]
        public void It_should_throw_an_exception_when_the_ILocalRequestProcessor_cant_send()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request };
            var processor = mocks.StrictMock<ILocalRequestProcessor>();
            var service = new ColomboService();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<ILocalRequestProcessor>().Instance(processor)
            );
            WcfServices.Kernel = kernel;

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(processor.CanProcess(request)).Return(false).Repeat.Twice();
            }).Verify(() =>
            {
                Exception testedException = null;

                var asyncResult = service.BeginProcessAsync(requests, (ar) =>
                {
                    try { service.EndProcessAsync(ar); }
                    catch (Exception ex) { testedException = ex; }
                }, null);

                Thread.Sleep(200);
                while (testedException == null) ;

                Assert.That(() => testedException,
                    Is.TypeOf<ColomboException>()
                    .With.Message.Contains("locally"));
            });
        }

        [Test]
        public void It_should_use_the_ILocalMessageProcessor_to_process_the_requests()
        {
            var mocks = new MockRepository();
            var request1 = mocks.Stub<Request<TestResponse>>();
            var request2 = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request1, request2 };

            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var responsesGroup = new ResponsesGroup
            {
                { request1, response1 },
                { request2, response2 }
            };

            var processor = mocks.StrictMock<ILocalRequestProcessor>();
            var service = new ColomboService();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<ILocalRequestProcessor>().Instance(processor)
            );
            WcfServices.Kernel = kernel;

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(processor.CanProcess(request1)).Return(true);
                Expect.Call(processor.CanProcess(request2)).Return(true);
                Expect.Call(processor.Process(null)).IgnoreArguments().Return(responsesGroup);
            }).Verify(() =>
            {
                Response[] responses = null;

                var asyncResult = service.BeginProcessAsync(requests, (ar) =>
                {
                    responses = service.EndProcessAsync(ar);
                }, null);

                Thread.Sleep(200);
                while (responses == null) ;

                Assert.That(() => responses[0],
                    Is.SameAs(response1));
                Assert.That(() => responses[1],
                    Is.SameAs(response2));
            });
        }

        [Test]
        public void It_should_handle_WCF_calls()
        {
            const string IPCAddress = @"net.pipe://localhost/ipctest";

            var mocks = new MockRepository();
            var request1 = new TestRequest();
            var request2 = new TestRequest();
            var requests = new BaseRequest[] { request1, request2 };

            var response1 = new TestResponse();
            var response2 = new TestResponse();

            var processor = mocks.DynamicMock<ILocalRequestProcessor>();
            var service = new ColomboService();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<ILocalRequestProcessor>().Instance(processor)
            );
            WcfServices.Kernel = kernel;

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(processor.CanProcess(null)).IgnoreArguments().Return(true);
                Expect.Call(processor.Process(null)).IgnoreArguments().Do(new ProcessDelegate((delegateRequests) =>
                {
                    return new ResponsesGroup
                    {
                        { delegateRequests[0], response1 },
                        { delegateRequests[1], response2 }
                    };
                }));
            }).Verify(() =>
            {
                using (var serviceHost = new ServiceHost(typeof(ColomboService), new Uri(IPCAddress)))
                {
                    serviceHost.Open();
                    var channelFactory = new ChannelFactory<IColomboService>(new NetNamedPipeBinding(), new EndpointAddress(IPCAddress));
                    var wcfService = channelFactory.CreateChannel();

                    var asyncResult = wcfService.BeginProcessAsync(requests, null, null);
                    asyncResult.AsyncWaitHandle.WaitOne();
                    var responses = wcfService.EndProcessAsync(asyncResult);

                    Assert.That(() => responses[0].CorrelationGuid,
                    Is.EqualTo(response1.CorrelationGuid));
                    Assert.That(() => responses[1].CorrelationGuid,
                        Is.EqualTo(response2.CorrelationGuid));
                    ((IClientChannel)wcfService).Close();
                }
            });
        }

        public delegate ResponsesGroup ProcessDelegate(IList<BaseRequest> requests);

        public class TestRequest : Request<TestResponse>
        {
        }
    }
}
