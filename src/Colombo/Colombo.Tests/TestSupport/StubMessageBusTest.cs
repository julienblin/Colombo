using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Colombo.TestSupport;
using NUnit.Framework;

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
                .Expect<TestRequest2, TestResponse2>()
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
                .Expect<TestRequest2, TestResponse2>()
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
                .Expect<TestSideEffectFreeRequest2, TestResponse2>()
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
                .Expect<TestSideEffectFreeRequest2, TestResponse2>()
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
                .Expect<TestSideEffectFreeRequest2, TestResponse2>()
                .Reply((request, response) => response.Name = request.Name);

            stubMessageBus.SendAsync(new TestSideEffectFreeRequest2() { Name = "TheName" }).Register(response =>
            {
                Assert.That(() => response,
                    Is.Not.Null.And.Property("Name").EqualTo("TheName"));
            });

            Assert.DoesNotThrow(() => stubMessageBus.Verify());
        }

        [Test]
        public void It_should_verify_expectations()
        {
            var stubMessageBus = new StubMessageBus();
            stubMessageBus
                .Expect<TestRequest2, TestResponse2>()
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

            Assert.That(() => stubMessageBus.Expect<TestRequest2, TestResponse2>(),
                Throws.Exception.TypeOf<ColomboTestSupportException>()
                .With.Message.Contains("TestRequest2"));

            container = new WindsorContainer();
            stubMessageBus = new StubMessageBus { Kernel = container.Kernel };

            stubMessageBus.Expect<TestRequest2, TestResponse2>();

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
                .Expect<TestRequest3, TestResponse2>()
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
                .Expect<TestRequest3, TestResponse2>()
                .Reply((request, response) => response.Name = "SubHandler");

            stubMessageBus.Send(new TestRequest2());

            Assert.That(() => stubMessageBus.Verify(),
                        Throws.Exception.TypeOf<ColomboExpectationException>()
                        .With.Message.Contains("TestRequest3"));
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
                var subResponse = MessageBus.Send(new TestRequest3());
                Response.Name = subResponse.Name;
            }
        }

        public class TestRequest3 : Request<TestResponse2>
        {
            
        }
    }
}
