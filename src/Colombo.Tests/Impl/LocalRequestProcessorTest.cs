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
using Colombo.Impl;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Impl
{
    [TestFixture]
    public class LocalRequestProcessorTest : BaseTest
    {
        [Test]
        public void It_should_ensure_that_it_has_a_IRequestHandlerFactory()
        {
            Assert.That(() => new LocalRequestProcessor(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("requestHandlerFactory"));
        }

        [Test]
        public void It_should_rely_on_IRequestHandlerFactory_for_CanProcess()
        {
            var mocks = new MockRepository();
            var request = new TestRequest();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestHandlerFactory.CanCreateRequestHandlerFor(request)).Return(false);
            }).Verify(() =>
            {
                var processor = new LocalRequestProcessor(requestHandlerFactory);
                processor.Logger = GetConsoleLogger();
                Assert.That(() => processor.CanProcess(request),
                    Is.False);
            });
        }

        [Test]
        public void It_should_ensure_that_IRequestHandlerFactory_returns_a_handler_in_Create()
        {
            var mocks = new MockRepository();
            var request = new TestRequest();
            var requests = new List<BaseRequest> { request };
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request)).Return(null);
            }).Verify(() =>
            {
                var processor = new LocalRequestProcessor(requestHandlerFactory);
                processor.Logger = GetConsoleLogger();
                Assert.That(() => processor.Process(requests),
                    Throws.Exception.TypeOf<ColomboException>());
            });
        }

        [Test]
        public void It_should_use_the_RequestHandlers_that_IRequestHandlerFactory_returns()
        {
            var mocks = new MockRepository();
            var request1 = new TestRequest();
            var request2 = new TestRequest();
            var requests = new List<BaseRequest> { request1, request2 };
            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();
            var requestHandler1 = new TestRequestHandler(response1);
            var requestHandler2 = new TestRequestHandler(response2);

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request1)).Return(requestHandler1);
                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request2)).Return(requestHandler2);
                requestHandlerFactory.DisposeRequestHandler(requestHandler1);
                requestHandlerFactory.DisposeRequestHandler(requestHandler2);
            }).Verify(() =>
            {
                var processor = new LocalRequestProcessor(requestHandlerFactory);
                processor.Logger = GetConsoleLogger();
                var responses = processor.Process(requests);
                Assert.That(() => responses[request1],
                    Is.SameAs(response1));
                Assert.That(() => responses[request2],
                    Is.SameAs(response2));
            });
        }

        public class TestRequest : Request<TestResponse>
        {}

        public class TestRequestHandler : RequestHandler<TestRequest, TestResponse>
        {
            private readonly TestResponse response;
            public BaseRequest ReceivedRequest { get; set; }

            public TestRequestHandler(TestResponse response)
            {
                this.response = response;
            }

            protected override void Handle()
            {
                ReceivedRequest = Request;
                Response = response;
            }
        }

        [Test]
        public void It_should_run_all_the_IRequestHandlerHandleInterceptors()
        {
            var mocks = new MockRepository();
            var request1 = new TestRequest();
            var request2 = new TestRequest();
            var requests = new List<BaseRequest> { request1, request2 };
            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();
            var requestHandler1 = mocks.StrictMock<IRequestHandler>();
            var requestHandler2 = mocks.StrictMock<IRequestHandler>();

            var interceptor1 = mocks.StrictMock<IRequestHandlerHandleInterceptor>();
            var interceptor2 = mocks.StrictMock<IRequestHandlerHandleInterceptor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(interceptor1.InterceptionPriority).Return(InterceptionPrority.High);
                Expect.Call(interceptor2.InterceptionPriority).Return(InterceptionPrority.Medium);

                interceptor1.Intercept(null);
                LastCall.IgnoreArguments().Repeat.Twice().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                interceptor2.Intercept(null);
                LastCall.IgnoreArguments().Repeat.Twice().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                    invocation.Response = response2;
                }));

                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request1)).Return(requestHandler1);
                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request2)).Return(requestHandler2);
                Expect.Call(requestHandler1.Handle(request1)).Return(response1);
                Expect.Call(requestHandler2.Handle(request2)).Return(response1);
                requestHandlerFactory.DisposeRequestHandler(requestHandler1);
                requestHandlerFactory.DisposeRequestHandler(requestHandler2);
            }).Verify(() =>
            {
                var processor = new LocalRequestProcessor(requestHandlerFactory);
                processor.Logger = GetConsoleLogger();
                processor.RequestHandlerInterceptors = new IRequestHandlerHandleInterceptor[] { interceptor1, interceptor2 };
                var responses = processor.Process(requests);
                Assert.That(() => responses[request1],
                    Is.SameAs(response2));
                Assert.That(() => responses[request2],
                    Is.SameAs(response2));
            });
        }

        [Test]
        public void It_should_include_ContextMeta()
        {
            var mocks = new MockRepository();
            var request1 = new TestRequest();
            var requests = new List<BaseRequest> { request1 };
            var response1 = new TestResponse();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();
            var requestHandler1 = new TestRequestHandler(response1);

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request1)).Return(requestHandler1);
                requestHandlerFactory.DisposeRequestHandler(requestHandler1);
            }).Verify(() =>
            {
                var processor = new LocalRequestProcessor(requestHandlerFactory);
                processor.Logger = GetConsoleLogger();
                processor.Process(requests);

                Assert.That(requestHandler1.ReceivedRequest.Context[ContextMeta.HandlerMachineName], Is.EqualTo(Environment.MachineName));
            });
        }

        [Test]
        public void It_should_not_include_ContextMeta_when_disabled()
        {
            var mocks = new MockRepository();
            var request1 = new TestRequest();
            var requests = new List<BaseRequest> { request1 };
            var response1 = new TestResponse();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();
            var requestHandler1 = new TestRequestHandler(response1);

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request1)).Return(requestHandler1);
                requestHandlerFactory.DisposeRequestHandler(requestHandler1);
            }).Verify(() =>
            {
                var processor = new LocalRequestProcessor(requestHandlerFactory)
                                    {
                                        DoNotManageContextMeta = true,
                                        Logger = GetConsoleLogger()
                                    };
                processor.Process(requests);

                Assert.That(requestHandler1.ReceivedRequest.Context.ContainsKey(ContextMeta.HandlerMachineName), Is.False);
            });
        }

        public delegate void InterceptDelegate(IColomboRequestHandleInvocation invocation);
    }
}
