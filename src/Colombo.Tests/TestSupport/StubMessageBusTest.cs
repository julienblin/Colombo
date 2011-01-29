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

using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Colombo.TestSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.TestSupport
{
    [TestFixture]
    public class StubMessageBusTest
    {
        [Test]
        public void It_should_work_with_expectation_SendRequest()
        {
            var stubMessageBus = new StubMessageBus();
            stubMessageBus
                .ExpectSend<TestRequest2, TestResponse2>()
                .Reply((request, response) => response.Name = request.Name);

            Assert.That(() => stubMessageBus.Send(new TestRequest2() { Name = "TheName" }),
                Is.Not.Null.And.Property("Name").EqualTo("TheName"));

            Assert.DoesNotThrow(() => stubMessageBus.Verify());
        }

        [Test]
        public void It_should_work_with_expectation_SendAsyncRequest()
        {
            var stubMessageBus = new StubMessageBus();
            stubMessageBus
                .ExpectSend<TestRequest2, TestResponse2>()
                .Reply((request, response) => response.Name = request.Name);

            stubMessageBus.SendAsync(new TestRequest2() { Name = "TheName" }).Register(response =>
            {
                Assert.That(() => response,
                    Is.Not.Null.And.Property("Name").EqualTo("TheName"));
            });

            Thread.Sleep(200);
            Assert.DoesNotThrow(() => stubMessageBus.Verify());
        }

        [Test]
        public void It_should_work_with_expectation_SendSideEffectFreeRequest()
        {
            var stubMessageBus = new StubMessageBus();
            stubMessageBus
                .ExpectSend<TestSideEffectFreeRequest2, TestResponse2>()
                .Reply((request, response) => response.Name = request.Name);

            Assert.That(() => stubMessageBus.Send(new TestSideEffectFreeRequest2() { Name = "TheName" }),
                Is.Not.Null.And.Property("Name").EqualTo("TheName"));

            Thread.Sleep(200);
            Assert.DoesNotThrow(() => stubMessageBus.Verify());
        }

        [Test]
        public void It_should_work_with_expectation_SendAction()
        {
            var stubMessageBus = new StubMessageBus();
            stubMessageBus
                .ExpectSend<TestSideEffectFreeRequest2, TestResponse2>()
                .Reply((request, response) => response.Name = request.Name);

            Assert.That(() => stubMessageBus.Send<TestSideEffectFreeRequest2, TestResponse2>(r => r.Name = "TheName"),
                Is.Not.Null.And.Property("Name").EqualTo("TheName"));

            Assert.DoesNotThrow(() => stubMessageBus.Verify());
        }

        [Test]
        public void It_should_work_with_expectation_SendAsyncSideEffectFreeRequest()
        {
            var stubMessageBus = new StubMessageBus();
            stubMessageBus
                .ExpectSend<TestSideEffectFreeRequest2, TestResponse2>()
                .Reply((request, response) => response.Name = request.Name);

            stubMessageBus.SendAsync(new TestSideEffectFreeRequest2() { Name = "TheName" }).Register(response =>
            {
                Assert.That(() => response,
                    Is.Not.Null.And.Property("Name").EqualTo("TheName"));
            });

            Thread.Sleep(200);
            Assert.DoesNotThrow(() => stubMessageBus.Verify());
        }

        [Test]
        public void It_should_work_with_expectation_SendParallel()
        {
            var stubMessageBus = new StubMessageBus();
            stubMessageBus
                .ExpectSend<TestSideEffectFreeRequest2, TestResponse2>()
                .Reply((request, response) => response.Name = request.Name)
                .Repeat(2);

            var request1 = new TestSideEffectFreeRequest2() { Name = "Req1" };
            var request2 = new TestSideEffectFreeRequest2() { Name = "Req2" };

            var responses = stubMessageBus.Send(request1, request2);

            Assert.That(() => responses.GetFrom(request1),
                Is.Not.Null.And.Property("Name").EqualTo("Req1"));

            Assert.That(() => responses.GetFrom(request2),
                Is.Not.Null.And.Property("Name").EqualTo("Req2"));

            Assert.DoesNotThrow(() => stubMessageBus.Verify());
        }

        [Test]
        public void It_should_verify_expectations()
        {
            var stubMessageBus = new StubMessageBus();
            stubMessageBus
                .ExpectSend<TestRequest2, TestResponse2>()
                .Reply((request, response) => response.Name = request.Name)
                .Repeat(2);

            stubMessageBus.Send(new TestRequest2() { Name = "TheName" });

            Assert.That(() => stubMessageBus.Verify(),
                Throws.Exception.TypeOf<ColomboExpectationException>()
                .With.Message.ContainsSubstring("TestRequest2")
                .And.Message.ContainsSubstring("1")
                .And.Message.ContainsSubstring("2"));
        }

        [Test]
        public void It_should_invoke_TestHandler_using_Kernel()
        {
            var container = new WindsorContainer();
            var stubMessageBus = new StubMessageBus { Kernel = container.Kernel };

            stubMessageBus.TestHandler<TestRequestHandler2>();

            Assert.That(() => stubMessageBus.Send(new TestRequest2() { Name = "TheName" }),
                Is.Not.Null.And.Property("Name").EqualTo("HandlerTheName"));

            Assert.DoesNotThrow(() => stubMessageBus.Verify());
        }

        [Test]
        public void It_should_not_allow_multiple_registration_for_requests()
        {
            var container = new WindsorContainer();
            var stubMessageBus = new StubMessageBus { Kernel = container.Kernel };
            stubMessageBus.TestHandler<TestRequestHandler2>();

            Assert.That(() => stubMessageBus.ExpectSend<TestRequest2, TestResponse2>(),
                Throws.Exception.TypeOf<ColomboTestSupportException>()
                .With.Message.Contains("TestRequest2"));

            container = new WindsorContainer();
            stubMessageBus = new StubMessageBus { Kernel = container.Kernel };

            stubMessageBus.ExpectSend<TestRequest2, TestResponse2>();

            Assert.That(() => stubMessageBus.TestHandler<TestRequestHandler2>(),
                Throws.Exception.TypeOf<ColomboTestSupportException>()
                .With.Message.Contains("TestRequest2")
                .And.Message.Contains("TestRequestHandler2"));
        }

        [Test]
        public void It_should_throw_exception_if_request_not_expected()
        {
            var stubMessageBus = new StubMessageBus();
            Assert.That(() => stubMessageBus.Send(new TestRequest2()),
                Throws.Exception.TypeOf<ColomboExpectationException>()
                .With.Message.Contains("TestRequest2"));
        }

        [Test]
        public void It_should_reply_with_default_responses_if_AllowUnexpectedMessages()
        {
            var stubMessageBus = new StubMessageBus();
            stubMessageBus.AllowUnexpectedMessages = true;

            var testRequest2 = new TestRequest2();
            Assert.That(() => stubMessageBus.Send(testRequest2),
                Is.Not.Null.And.Property("CorrelationGuid").EqualTo(testRequest2.CorrelationGuid));
        }

        [Test]
        public void It_should_not_throw_if_sub_expectations_are_met()
        {
            var container = new WindsorContainer();
            var stubMessageBus = new StubMessageBus { Kernel = container.Kernel };
            container.Register(Component.For<IStubMessageBus, IMessageBus>().Instance(stubMessageBus));

            stubMessageBus
                .ExpectSend<TestRequest3, TestResponse2>()
                .Reply((request, response) => response.Name = "SubHandler");

            stubMessageBus.TestHandler<TestRequestHandler22>();

            Assert.That(() => stubMessageBus.Send(new TestRequest2()),
                        Is.Not.Null.And.Property("Name").EqualTo("SubHandler"));

            Assert.DoesNotThrow(() => stubMessageBus.Verify());
        }

        [Test]
        public void It_should_throw_if_sub_expectations_are_not_met()
        {
            var container = new WindsorContainer();
            var stubMessageBus = new StubMessageBus { Kernel = container.Kernel };
            container.Register(Component.For<IStubMessageBus, IMessageBus>().Instance(stubMessageBus));

            stubMessageBus.TestHandler<TestRequestHandler22>();

            Assert.That(() => stubMessageBus.Send(new TestRequest2()),
                        Throws.Exception.TypeOf<ColomboExpectationException>()
                        .With.Message.Contains("TestRequest3"));

            container = new WindsorContainer();
            stubMessageBus = new StubMessageBus { Kernel = container.Kernel };
            container.Register(Component.For<IStubMessageBus, IMessageBus>().Instance(stubMessageBus));
            stubMessageBus.TestHandler<TestRequestHandler2>();
            stubMessageBus
                .ExpectSend<TestRequest3, TestResponse2>()
                .Reply((request, response) => response.Name = "SubHandler");

            stubMessageBus.Send(new TestRequest2());

            Assert.That(() => stubMessageBus.Verify(),
                        Throws.Exception.TypeOf<ColomboExpectationException>()
                        .With.Message.Contains("TestRequest3"));
        }

        [Test]
        public void It_should_use_registered_send_interceptors()
        {
            var mocks = new MockRepository();

            var interceptor = mocks.StrictMock<IMessageBusSendInterceptor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(interceptor.InterceptionPriority).Return(InterceptionPrority.High);
                interceptor.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptSendDelegate(invocation => invocation.Proceed()));
            }).Verify(() =>
            {
                var stubMessageBus = new StubMessageBus();
                stubMessageBus.MessageBusSendInterceptors = new[] { interceptor };
                stubMessageBus
                    .ExpectSend<TestRequest2, TestResponse2>()
                    .Reply((request, response) => response.Name = request.Name);

                Assert.That(() => stubMessageBus.Send(new TestRequest2() { Name = "TheName" }),
                    Is.Not.Null.And.Property("Name").EqualTo("TheName"));

                Assert.DoesNotThrow(() => stubMessageBus.Verify());
            });
        }

        [Test]
        public void It_should_use_registered_handle_requests_interceptors()
        {
            var mocks = new MockRepository();

            var interceptor = mocks.StrictMock<IRequestHandlerHandleInterceptor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(interceptor.InterceptionPriority).Return(InterceptionPrority.High);
                interceptor.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptRequestHandleDelegate(invocation => invocation.Proceed()));
            }).Verify(() =>
            {
                var stubMessageBus = new StubMessageBus();
                stubMessageBus.RequestHandlerInterceptors = new[] { interceptor };
                stubMessageBus
                    .ExpectSend<TestRequest2, TestResponse2>()
                    .Reply((request, response) => response.Name = request.Name);

                Assert.That(() => stubMessageBus.Send(new TestRequest2() { Name = "TheName" }),
                    Is.Not.Null.And.Property("Name").EqualTo("TheName"));

                Assert.DoesNotThrow(() => stubMessageBus.Verify());
            });
        }

        [Test]
        public void It_should_accept_expectations_for_interceptors()
        {
            var mocks = new MockRepository();

            var interceptor = mocks.StrictMock<IRequestHandlerHandleInterceptor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(interceptor.InterceptionPriority).Return(InterceptionPrority.High);
                interceptor.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptRequestHandleDelegate(invocation =>
                {
                    invocation.Response = new TestResponse2();
                }));
            }).Verify(() =>
            {
                var container = new WindsorContainer();
                var stubMessageBus = new StubMessageBus { Kernel = container.Kernel };
                container.Register(Component.For<IStubMessageBus, IMessageBus>().Instance(stubMessageBus));
                stubMessageBus.RequestHandlerInterceptors = new[] { interceptor };
                stubMessageBus.TestHandler<TestRequestHandler2>().ShouldBeInterceptedBeforeHandling();

                stubMessageBus.Send(new TestRequest2() { Name = "TheName" });

                Assert.DoesNotThrow(() => stubMessageBus.Verify());
            });
        }

        public class TestResponse2 : Response
        {
            public virtual string Name { get; set; }
        }

        public class TestRequest2 : Request<TestResponse2>
        {
            public string Name { get; set; }
        }

        public class TestRequestHandler2 : RequestHandler<TestRequest2, TestResponse2>
        {
            protected override void Handle()
            {
                Response.Name = "Handler" + Request.Name;
            }
        }
        public class TestSideEffectFreeRequest2 : SideEffectFreeRequest<TestResponse2>
        {
            public string Name { get; set; }
        }

        public class TestRequestHandler22 : RequestHandler<TestRequest2, TestResponse2>
        {
            public IMessageBus MessageBus { get; set; }

            protected override void Handle()
            {
                var subRequest = CreateRequest<TestRequest3>();
                var subResponse = MessageBus.Send(subRequest);
                Response.Name = subResponse.Name;
            }
        }

        public class TestRequest3 : Request<TestResponse2>
        {
            public string Name { get; set; }
        }

        public delegate void InterceptSendDelegate(IColomboSendInvocation invocation);
        public delegate void InterceptRequestHandleDelegate(IColomboRequestHandleInvocation nextInvocation);
    }
}
