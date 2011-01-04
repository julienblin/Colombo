using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Impl;
using Rhino.Mocks;

namespace Colombo.Tests.Impl
{
    [TestFixture]
    public class MessageBusSendTest
    {
        [Test]
        public void It_should_ensure_that_at_least_one_IMessageProcessor_is_provided()
        {
            Assert.That(() => new MessageBus(null),
                Throws.Exception.TypeOf<ArgumentException>()
                .With.Message.Contains("messageProcessors"));

            Assert.That(() => new MessageBus(new IMessageProcessor[] { }),
                Throws.Exception.TypeOf<ArgumentException>()
                .With.Message.Contains("messageProcessors"));
        }

        [Test]
        public void It_should_throw_an_exception_when_no_IMessageProcessor_can_send()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();

            var messageProcessor = mocks.DynamicMock<IMessageProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(messageProcessor.CanSend(request)).Return(false);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor });
                Assert.That(() => messageBus.Send(request),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains(messageProcessor.GetType().Name));
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_too_many_IMessageProcessors_can_send()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();

            var messageProcessor1 = mocks.StrictMock<IMessageProcessor>();
            var messageProcessor2 = mocks.StrictMock<IMessageProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(messageProcessor1.CanSend(request)).Return(true);
                Expect.Call(messageProcessor2.CanSend(request)).Return(true);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor1, messageProcessor2 });
                Assert.That(() => messageBus.Send<TestResponse>(request),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains(messageProcessor1.GetType().Name)
                    .With.Message.Contains(messageProcessor2.GetType().Name));
            });
        }

        [Test]
        public void It_should_call_selected_IMessageProcessors_Send_method()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var response = new TestResponse();

            var messageProcessor1 = mocks.StrictMock<IMessageProcessor>();
            var messageProcessor2 = mocks.StrictMock<IMessageProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(messageProcessor1.CanSend(request)).Return(false);
                Expect.Call(messageProcessor2.CanSend(request)).Return(true);
                Expect.Call(messageProcessor2.Send(request)).Return(response);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor1, messageProcessor2 });
                Assert.That(() => messageBus.Send<TestResponse>(request),
                    Is.SameAs(response));
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_incompatible_responses_are_returned()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var response = new TestResponse2();

            var messageProcessor = mocks.StrictMock<IMessageProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(messageProcessor.CanSend(request)).Return(true);
                Expect.Call(messageProcessor.Send(request)).Return(response);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor });
                Assert.That(() => messageBus.Send<TestResponse>(request),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains(typeof(TestResponse).ToString())
                    .With.Message.Contains(typeof(TestResponse2).ToString()));
            });
        }

        [Test]
        public void It_should_run_all_the_IMessageBusSendInterceptors()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var response = new TestResponse();
            var messageProcessor = mocks.StrictMock<IMessageProcessor>();
            var sendInterceptor1 = mocks.StrictMock<IMessageBusSendInterceptor>();
            var sendInterceptor2 = mocks.StrictMock<IMessageBusSendInterceptor>();
            var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor });

            With.Mocks(mocks).ExpectingInSameOrder(() =>
            {
                Expect.Call(sendInterceptor1.InterceptionPriority).Return(InterceptorPrority.High);
                Expect.Call(sendInterceptor2.InterceptionPriority).Return(InterceptorPrority.Medium);

                Expect.Call(messageProcessor.CanSend(request)).Return(true);

                sendInterceptor1.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                sendInterceptor2.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                Expect.Call(messageProcessor.Send(request)).Return(response);
            }).Verify(() =>
            {
                messageBus.MessageBusSendInterceptors = new IMessageBusSendInterceptor[] { sendInterceptor1, sendInterceptor2 };
                Assert.That(() => messageBus.Send<TestResponse>(request),
                    Is.SameAs(response));
            });
        }

        public delegate void InterceptDelegate(IColomboInvocation invocation);

        [Test]
        public void It_should_reorder_IMessageBusSendInterceptor_accordingly()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var response = new TestResponse();
            var messageProcessor = mocks.StrictMock<IMessageProcessor>();
            var sendInterceptor1 = mocks.StrictMock<IMessageBusSendInterceptor>();
            var sendInterceptor2 = mocks.StrictMock<IMessageBusSendInterceptor>();
            var sendInterceptor3 = mocks.StrictMock<IMessageBusSendInterceptor>();
            var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor });

            With.Mocks(mocks).ExpectingInSameOrder(() =>
            {
                Expect.Call(sendInterceptor1.InterceptionPriority).Return(InterceptorPrority.Low);
                Expect.Call(sendInterceptor2.InterceptionPriority).Return(InterceptorPrority.High);
                Expect.Call(sendInterceptor3.InterceptionPriority).Return(InterceptorPrority.Medium);

                Expect.Call(messageProcessor.CanSend(request)).Return(true);

                sendInterceptor2.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                sendInterceptor3.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                sendInterceptor1.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                Expect.Call(messageProcessor.Send(request)).Return(response);
            }).Verify(() =>
            {
                messageBus.MessageBusSendInterceptors = new IMessageBusSendInterceptor[] { sendInterceptor1, sendInterceptor2, sendInterceptor3 };
                Assert.That(() => messageBus.Send<TestResponse>(request),
                    Is.SameAs(response));
            });
        }

        public class TestResponse2 : Response
        {

        }
    }
}
