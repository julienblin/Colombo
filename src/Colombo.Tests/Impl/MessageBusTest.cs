using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Impl;
using Rhino.Mocks;
using System.Threading;

namespace Colombo.Tests.Impl
{
    [TestFixture]
    public class MessageBusTest : BaseTest
    {
        [Test]
        public void It_should_ensure_that_at_least_one_IRequestProcessor_is_provided()
        {
            Assert.That(() => new MessageBus(null),
                Throws.Exception.TypeOf<ArgumentException>()
                .With.Message.Contains("requestProcessors"));

            Assert.That(() => new MessageBus(new IRequestProcessor[] { }),
                Throws.Exception.TypeOf<ArgumentException>()
                .With.Message.Contains("requestProcessors"));
        }

        [Test]
        public void It_should_throw_an_exception_when_no_IRequestProcessor_can_process()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();

            var requestProcessor = mocks.DynamicMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor.CanProcess(request)).Return(false);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor });
                messageBus.Logger = GetConsoleLogger();
                Assert.That(() => messageBus.Send(request),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains(requestProcessor.GetType().Name));
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_no_IRequestProcessor_can_process_with_multiple_requests()
        {
            var mocks = new MockRepository();
            var request1 = mocks.Stub<SideEffectFreeRequest<TestResponse>>();
            var request2 = mocks.Stub<SideEffectFreeRequest<TestResponse>>();

            var requestProcessor1 = mocks.DynamicMock<IRequestProcessor>();
            var requestProcessor2 = mocks.DynamicMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor1.CanProcess(request1)).Return(false);
                Expect.Call(requestProcessor1.CanProcess(request2)).Return(false);
                Expect.Call(requestProcessor2.CanProcess(request1)).Return(true);
                Expect.Call(requestProcessor2.CanProcess(request2)).Return(false);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 });
                messageBus.Logger = GetConsoleLogger();
                Assert.That(() => messageBus.Send(request1, request2),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains(requestProcessor1.GetType().Name)
                    .With.Message.Contains(requestProcessor2.GetType().Name));
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_too_many_IRequestProcessors_can_process()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();

            var requestProcessor1 = mocks.StrictMock<IRequestProcessor>();
            var requestProcessor2 = mocks.StrictMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor1.CanProcess(request)).Return(true);
                Expect.Call(requestProcessor2.CanProcess(request)).Return(true);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 });
                messageBus.Logger = GetConsoleLogger();
                Assert.That(() => messageBus.Send(request),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains(requestProcessor1.GetType().Name)
                    .With.Message.Contains(requestProcessor2.GetType().Name));
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_too_many_IRequestProcessors_can_process_multiple_requests()
        {
            var mocks = new MockRepository();
            var request1 = mocks.Stub<SideEffectFreeRequest<TestResponse>>();
            var request2 = mocks.Stub<SideEffectFreeRequest<TestResponse>>();

            var requestProcessor1 = mocks.StrictMock<IRequestProcessor>();
            var requestProcessor2 = mocks.StrictMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor1.CanProcess(request1)).Return(false);
                Expect.Call(requestProcessor1.CanProcess(request2)).Return(true);
                Expect.Call(requestProcessor2.CanProcess(request1)).Return(true);
                Expect.Call(requestProcessor2.CanProcess(request2)).Return(true);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 });
                messageBus.Logger = GetConsoleLogger();
                Assert.That(() => messageBus.Send(request1, request2),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains(requestProcessor1.GetType().Name)
                    .With.Message.Contains(requestProcessor2.GetType().Name));
            });
        }

        [Test]
        public void It_should_call_selected_IRequestProcessors_Process_method()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request };
            var response = new TestResponse();
            var responses = new ResponsesGroup
            {
                { request, response}
            };

            var requestProcessor1 = mocks.StrictMock<IRequestProcessor>();
            var requestProcessor2 = mocks.StrictMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor1.CanProcess(request)).Return(false);
                Expect.Call(requestProcessor2.CanProcess(request)).Return(true);
                Expect.Call(requestProcessor2.Process(requests)).Return(responses);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 });
                messageBus.Logger = GetConsoleLogger();
                Assert.That(() => messageBus.Send(request),
                    Is.SameAs(response));
            });
        }

        [Test]
        public void It_should_call_selected_IRequestProcessors_Process_method_with_sideeffectfree()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<SideEffectFreeRequest<TestResponse>>();
            var requests = new BaseRequest[] { request };
            var response = new TestResponse();
            var responses = new ResponsesGroup
            {
                { request, response}
            };

            var requestProcessor1 = mocks.StrictMock<IRequestProcessor>();
            var requestProcessor2 = mocks.StrictMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor1.CanProcess(request)).Return(false);
                Expect.Call(requestProcessor2.CanProcess(request)).Return(true);
                Expect.Call(requestProcessor2.Process(requests)).Return(responses);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 });
                messageBus.Logger = GetConsoleLogger();
                Assert.That(() => messageBus.Send(request),
                    Is.SameAs(response));
            });
        }

        [Test]
        public void It_should_call_selected_IRequestProcessors_Process_method_multiple_requests()
        {
            var mocks = new MockRepository();
            var request1 = mocks.Stub<SideEffectFreeRequest<TestResponse>>();
            var request2 = mocks.Stub<SideEffectFreeRequest<TestResponse>>();
            var request3 = mocks.Stub<SideEffectFreeRequest<TestResponse>>();
            var request4 = mocks.Stub<SideEffectFreeRequest<TestResponse>>();
            var requestsForProcessor1 = new BaseRequest[] { request1, request3 };
            var requestsForProcessor2 = new BaseRequest[] { request2, request4 };
            var response1 = new TestResponse();
            response1.CorrelationGuid = request1.CorrelationGuid;
            var response2 = new TestResponse();
            response2.CorrelationGuid = request2.CorrelationGuid;
            var response3 = new TestResponse();
            response3.CorrelationGuid = request3.CorrelationGuid;
            var response4 = new TestResponse();
            response4.CorrelationGuid = request4.CorrelationGuid;
            var responsesForProcessor1 = new ResponsesGroup
            {
                { request1, response1 },
                { request3, response3 },
            };
            var responsesForProcessor2 = new ResponsesGroup
            {
                { request2, response2 },
                { request4, response4 },
            };

            var requestProcessor1 = mocks.StrictMock<IRequestProcessor>();
            var requestProcessor2 = mocks.StrictMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor1.CanProcess(request1)).Return(true);
                Expect.Call(requestProcessor1.CanProcess(request2)).Return(false);
                Expect.Call(requestProcessor1.CanProcess(request3)).Return(true);
                Expect.Call(requestProcessor1.CanProcess(request4)).Return(false);

                Expect.Call(requestProcessor2.CanProcess(request1)).Return(false);
                Expect.Call(requestProcessor2.CanProcess(request2)).Return(true);
                Expect.Call(requestProcessor2.CanProcess(request3)).Return(false);
                Expect.Call(requestProcessor2.CanProcess(request4)).Return(true);

                Expect.Call(requestProcessor1.Process(requestsForProcessor1)).Return(responsesForProcessor1);
                Expect.Call(requestProcessor2.Process(requestsForProcessor2)).Return(responsesForProcessor2);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 });
                messageBus.Logger = GetConsoleLogger();
                var responses = messageBus.Send(request1, request2, request3, request4);
                Assert.That(() => responses[request1],
                    Is.SameAs(response1));
                Assert.That(() => responses[request2],
                    Is.SameAs(response2));
                Assert.That(() => responses[request3],
                    Is.SameAs(response3));
                Assert.That(() => responses[request4],
                    Is.SameAs(response4));
            });
        }

        [Test]
        public void It_should_run_all_the_IMessageBusSendInterceptors()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request };
            var newCorrelationGuid = Guid.NewGuid();
            var responseFromProcessor = new TestResponse();
            var responsesFromProcessor = new ResponsesGroup
            {
                { request, responseFromProcessor}
            };

            var responseFromInterceptor = new TestResponse();
            var responsesFromInterceptor = new ResponsesGroup
            {
                { request, responseFromInterceptor}
            };

            var requestProcessor = mocks.StrictMock<IRequestProcessor>();

            var interceptor1 = mocks.StrictMock<IMessageBusSendInterceptor>();
            var interceptor2 = mocks.StrictMock<IMessageBusSendInterceptor>();

            With.Mocks(mocks).ExpectingInSameOrder(() =>
            {
                Expect.Call(interceptor1.InterceptionPriority).Return(InterceptorPrority.High);
                Expect.Call(interceptor2.InterceptionPriority).Return(InterceptorPrority.Medium);

                interceptor1.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    Assert.That(() => invocation.Requests[0].CorrelationGuid,
                        Is.EqualTo(request.CorrelationGuid));
                    invocation.Requests[0].CorrelationGuid = newCorrelationGuid;
                    invocation.Proceed();
                }));

                interceptor2.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    Assert.That(() => invocation.Requests[0].CorrelationGuid,
                        Is.EqualTo(newCorrelationGuid));
                    invocation.Proceed();
                    invocation.Responses = responsesFromInterceptor;
                }));

                Expect.Call(requestProcessor.CanProcess(request)).Return(true);
                Expect.Call(requestProcessor.Process(requests)).Return(responsesFromProcessor);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor });
                messageBus.Logger = GetConsoleLogger();
                messageBus.MessageBusSendInterceptors = new IMessageBusSendInterceptor[] { interceptor1, interceptor2 };
                Assert.That(() => messageBus.Send(request),
                    Is.SameAs(responseFromInterceptor));
            });
        }

        [Test]
        public void It_should_run_all_the_IMessageBusSendInterceptors_multiple_requests()
        {
            var mocks = new MockRepository();
            var request1 = mocks.Stub<SideEffectFreeRequest<TestResponse>>();
            var request2 = mocks.Stub<SideEffectFreeRequest<TestResponse>>();
            var requests = new BaseRequest[] { request1, request2 };
            var newCorrelationGuid = Guid.NewGuid();
            
            var responseFromProcessor1 = new TestResponse();
            var responseFromProcessor2 = new TestResponse();
            var responsesFromProcessor = new ResponsesGroup
            {
                { request1, responseFromProcessor1},
                { request2, responseFromProcessor2}
            };

            var responseFromInterceptor1 = new TestResponse();
            var responseFromInterceptor2 = new TestResponse();
            var responsesFromInterceptor = new ResponsesGroup
            {
                { request1, responseFromInterceptor1},
                { request2, responseFromInterceptor2}
            };

            var requestProcessor = mocks.StrictMock<IRequestProcessor>();

            var interceptor1 = mocks.StrictMock<IMessageBusSendInterceptor>();
            var interceptor2 = mocks.StrictMock<IMessageBusSendInterceptor>();

            With.Mocks(mocks).ExpectingInSameOrder(() =>
            {
                Expect.Call(interceptor1.InterceptionPriority).Return(InterceptorPrority.Low);
                Expect.Call(interceptor2.InterceptionPriority).Return(InterceptorPrority.High);

                interceptor2.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    Assert.That(() => invocation.Requests[0].CorrelationGuid,
                        Is.EqualTo(request1.CorrelationGuid));
                    Assert.That(() => invocation.Requests[1].CorrelationGuid,
                        Is.EqualTo(request2.CorrelationGuid));
                    invocation.Requests[1].CorrelationGuid = newCorrelationGuid;
                    invocation.Proceed();
                }));

                interceptor1.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    Assert.That(() => invocation.Requests[0].CorrelationGuid,
                        Is.EqualTo(request1.CorrelationGuid));
                    Assert.That(() => invocation.Requests[1].CorrelationGuid,
                        Is.EqualTo(newCorrelationGuid));
                    invocation.Proceed();
                    invocation.Responses = responsesFromInterceptor;
                }));

                Expect.Call(requestProcessor.CanProcess(request1)).Return(true);
                Expect.Call(requestProcessor.CanProcess(request2)).Return(true);
                Expect.Call(requestProcessor.Process(requests)).Return(responsesFromProcessor);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor });
                messageBus.Logger = GetConsoleLogger();
                messageBus.MessageBusSendInterceptors = new IMessageBusSendInterceptor[] { interceptor1, interceptor2 };
                Assert.That(() => messageBus.Send(request1, request2),
                    Is.SameAs(responsesFromInterceptor));
            });
        }

        [Test]
        public void It_should_be_able_to_send_asynchronously()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request };
            var response = new TestResponse();
            var responses = new ResponsesGroup
            {
                { request, response}
            };

            var requestProcessor = mocks.StrictMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor.CanProcess(request)).Return(true);
                Expect.Call(requestProcessor.Process(requests)).Return(responses);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor });
                messageBus.Logger = GetConsoleLogger();
                var callbackThreadId = 0;
                messageBus.SendAsync(request).Register(r =>
                {
                    Assert.AreSame(r, response);
                    callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                });
                Thread.Sleep(50);
                Assert.That(() => callbackThreadId,
                    Is.Not.EqualTo(Thread.CurrentThread.ManagedThreadId));
            });
        }

        [Test]
        public void It_should_be_able_to_handle_exceptions_asynchronously()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request };
            var response = new TestResponse();
            var responses = new ResponsesGroup
            {
                { request, response}
            };

            var requestProcessor = mocks.StrictMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor.CanProcess(request)).Return(true);
                Expect.Call(requestProcessor.Process(requests)).Throw(new Exception("Test exception"));
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor });
                messageBus.Logger = GetConsoleLogger();
                var callbackThreadId = 0;
                messageBus.SendAsync(request).Register(r =>
                {
                    Assert.Fail();
                },
                e =>
                {
                    Assert.That(() => e.Message,
                        Is.EqualTo("Test exception"));
                    callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                });
                Thread.Sleep(50);
                Assert.That(() => callbackThreadId,
                    Is.Not.EqualTo(Thread.CurrentThread.ManagedThreadId));
            });
        }

        public delegate void InterceptDelegate(IColomboSendInvocation invocation);
    }
}
