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
    public class MessageBusParallelSendTest
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
            var request1 = mocks.Stub<Request<TestResponse>>();
            var request2 = mocks.Stub<Request<TestResponse>>();

            var messageProcessor = mocks.DynamicMock<IMessageProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(messageProcessor.CanSend(request1)).Return(false);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor });
                Assert.That(() => messageBus.ParallelSend(request1, request2),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains(messageProcessor.GetType().Name));
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_too_many_IMessageProcessors_can_send()
        {
            var mocks = new MockRepository();
            var request1 = mocks.Stub<Request<TestResponse>>();
            var request2 = mocks.Stub<Request<TestResponse>>();

            var messageProcessor1 = mocks.StrictMock<IMessageProcessor>();
            var messageProcessor2 = mocks.StrictMock<IMessageProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(messageProcessor1.CanSend(request1)).Return(true);
                Expect.Call(messageProcessor2.CanSend(request1)).Return(true);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor1, messageProcessor2 });
                Assert.That(() => messageBus.ParallelSend(request1, request2),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains(messageProcessor1.GetType().Name)
                    .With.Message.Contains(messageProcessor2.GetType().Name));
            });
        }

        [Test]
        public void It_should_call_selected_IMessageProcessors_ParallelSend_method_when_there_is_one_processor()
        {
            var mocks = new MockRepository();

            var request1 = mocks.Stub<Request<TestResponse>>();
            var request2 = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request1, request2 };

            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var responses = new Response[] { response1, response2 };

            var messageProcessor1 = mocks.StrictMock<IMessageProcessor>();
            var messageProcessor2 = mocks.StrictMock<IMessageProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(messageProcessor1.CanSend(request1)).Return(false);
                Expect.Call(messageProcessor1.CanSend(request2)).Return(false);

                Expect.Call(messageProcessor2.CanSend(request1)).Return(true);
                Expect.Call(messageProcessor2.CanSend(request2)).Return(true);

                Expect.Call(messageProcessor2.ParallelSend(requests)).Return(responses);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor1, messageProcessor2 });
                var responseGroup = messageBus.ParallelSend(request1, request2);
                Assert.That(() => responseGroup.First,
                    Is.SameAs(response1));
                Assert.That(() => responseGroup.Second,
                    Is.SameAs(response2));
            });
        }

        [Test]
        public void It_should_call_selected_IMessageProcessors_ParallelSend_method_when_there_are_multiple_processors()
        {
            var mocks = new MockRepository();

            var request1 = mocks.Stub<Request<TestResponse>>();
            var request2 = mocks.Stub<Request<TestResponse>>();
            var request3 = mocks.Stub<Request<TestResponse>>();
            var request4 = mocks.Stub<Request<TestResponse>>();
            var requestsForMessageProcessor1 = new BaseRequest[] { request2, request4 };
            var requestsForMessageProcessor2 = new BaseRequest[] { request1, request3 };

            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var response3 = new TestResponse();
            var response4 = new TestResponse();
            var responsesForMessageProcessor1 = new Response[] { response2, response4 };
            var responsesForMessageProcessor2 = new Response[] { response1, response3 };

            var messageProcessor1 = mocks.StrictMock<IMessageProcessor>();
            var messageProcessor2 = mocks.StrictMock<IMessageProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(messageProcessor1.CanSend(request1)).Return(false);
                Expect.Call(messageProcessor1.CanSend(request2)).Return(true);
                Expect.Call(messageProcessor1.CanSend(request3)).Return(false);
                Expect.Call(messageProcessor1.CanSend(request4)).Return(true);

                Expect.Call(messageProcessor2.CanSend(request1)).Return(true);
                Expect.Call(messageProcessor2.CanSend(request2)).Return(false);
                Expect.Call(messageProcessor2.CanSend(request3)).Return(true);
                Expect.Call(messageProcessor2.CanSend(request4)).Return(false);

                Expect.Call(messageProcessor1.ParallelSend(requestsForMessageProcessor1)).Return(responsesForMessageProcessor1);
                Expect.Call(messageProcessor2.ParallelSend(requestsForMessageProcessor2)).Return(responsesForMessageProcessor2);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor1, messageProcessor2 });
                var responseGroup = messageBus.ParallelSend(request1, request2, request3, request4);
                Assert.That(() => responseGroup.First,
                    Is.SameAs(response1));
                Assert.That(() => responseGroup.Second,
                    Is.SameAs(response2));
                Assert.That(() => responseGroup.Third,
                    Is.SameAs(response3));
                Assert.That(() => responseGroup.Fourth,
                    Is.SameAs(response4));
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_incompatible_responses_are_returned()
        {
            var mocks = new MockRepository();
            var request1 = mocks.Stub<Request<TestResponse>>();
            var request2 = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request1, request2 };

            var response1 = new TestResponse();
            var response2 = new TestResponse2();
            var responses = new Response[] { response1, response2 };

            var messageProcessor = mocks.StrictMock<IMessageProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(messageProcessor.CanSend(request1)).Return(true);
                Expect.Call(messageProcessor.CanSend(request2)).Return(true);
                Expect.Call(messageProcessor.ParallelSend(requests)).Return(responses);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor });
                Assert.That(() => messageBus.ParallelSend(request1, request2),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains(typeof(TestResponse).ToString())
                    .With.Message.Contains(typeof(TestResponse2).ToString()));
            });
        }

        [Test]
        public void It_should_run_all_the_IMessageBusSendInterceptors()
        {
            var mocks = new MockRepository();
            var request1 = mocks.Stub<Request<TestResponse>>();
            var request2 = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request1, request2 };

            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var responses = new Response[] { response1, response2 };

            var messageProcessor = mocks.StrictMock<IMessageProcessor>();
            var sendInterceptor1 = mocks.StrictMock<IMessageBusSendInterceptor>();
            var sendInterceptor2 = mocks.StrictMock<IMessageBusSendInterceptor>();
            var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor });

            With.Mocks(mocks).ExpectingInSameOrder(() =>
            {
                Expect.Call(sendInterceptor1.InterceptionPriority).Return(InterceptorPrority.High);
                Expect.Call(sendInterceptor2.InterceptionPriority).Return(InterceptorPrority.Medium);

                IColomboParallelInvocation parallelInvocation = null;
                sendInterceptor1.Intercept(parallelInvocation);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                sendInterceptor2.Intercept(parallelInvocation);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                Expect.Call(messageProcessor.CanSend(request1)).Return(true);
                Expect.Call(messageProcessor.CanSend(request2)).Return(true);

                Expect.Call(messageProcessor.ParallelSend(requests)).Return(responses);
            }).Verify(() =>
            {
                messageBus.MessageBusSendInterceptors = new IMessageBusSendInterceptor[] { sendInterceptor1, sendInterceptor2 };
                var responseGroup = messageBus.ParallelSend(request1, request2);
                Assert.That(() => responseGroup.First,
                    Is.SameAs(response1));
                Assert.That(() => responseGroup.Second,
                    Is.SameAs(response2));
            });
        }

        public delegate void InterceptDelegate(IColomboParallelInvocation invocation);

        public class TestResponse2 : Response
        {

        }
    }
}
