using System;
using System.Collections.Generic;
using System.Threading;
using Colombo.Impl;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Impl
{
    [TestFixture]
    public class MessageBusTest : BaseTest
    {
        [Test]
        public void It_should_ensure_that_at_least_one_IRequestProcessor_is_provided()
        {
            var mocks = new MockRepository();
            var requestProcessor = mocks.Stub<IRequestProcessor>();
            var notificationProcessor = mocks.Stub<INotificationProcessor>();

            Assert.That(() => new MessageBus(null),
                Throws.Exception.TypeOf<ArgumentException>()
                .With.Message.Contains("requestProcessors"));

            Assert.That(() => new MessageBus(new IRequestProcessor[] { }, null),
                Throws.Exception.TypeOf<ArgumentException>()
                .With.Message.Contains("requestProcessors"));

            var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor }, null);
        }

        [Test]
        public void It_should_throw_an_exception_when_no_IRequestProcessor_can_process()
        {
            var mocks = new MockRepository();
            var notificationProcessor = mocks.Stub<INotificationProcessor>();

            var request = mocks.Stub<Request<TestResponse>>();

            var requestProcessor = mocks.DynamicMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor.CanProcess(request)).Return(false);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor }, new INotificationProcessor[] { notificationProcessor });
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
            var notificationProcessor = mocks.Stub<INotificationProcessor>();
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
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 }, new INotificationProcessor[] { notificationProcessor });
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
            var notificationProcessor = mocks.Stub<INotificationProcessor>();
            var request = mocks.Stub<Request<TestResponse>>();

            var requestProcessor1 = mocks.StrictMock<IRequestProcessor>();
            var requestProcessor2 = mocks.StrictMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor1.CanProcess(request)).Return(true);
                Expect.Call(requestProcessor2.CanProcess(request)).Return(true);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 }, new INotificationProcessor[] { notificationProcessor });
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
            var notificationProcessor = mocks.Stub<INotificationProcessor>();
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
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 }, new INotificationProcessor[] { notificationProcessor });
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
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 }, null);
                messageBus.Logger = GetConsoleLogger();
                Assert.That(() => messageBus.Send(request),
                    Is.SameAs(response));
            });
        }

        [Test]
        public void It_should_create_a_request_when_using_action_send()
        {
            var mocks = new MockRepository();
            var response = new TestResponse();

            var requestProcessor = mocks.StrictMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor.CanProcess(null)).IgnoreArguments().Return(true);
                Expect.Call(requestProcessor.Process(null)).IgnoreArguments().Do(new ProcessDelegate((rs) =>
                {
                    return new ResponsesGroup
                    {
                        { rs[0], response}
                    };
                }));
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor }, null);
                messageBus.Logger = GetConsoleLogger();
                var responseTest = messageBus.Send<TestRequestForSendAction, TestResponse>(r =>
                {
                    Assert.That(() => r, Is.Not.Null);
                });
                Assert.That(() => responseTest,
                    Is.SameAs(response));
            });
        }

        [Test]
        public void It_should_call_selected_IRequestProcessors_Process_method_with_sideeffectfree()
        {
            var mocks = new MockRepository();
            var notificationProcessor = mocks.Stub<INotificationProcessor>();
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
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 }, new INotificationProcessor[] { notificationProcessor });
                messageBus.Logger = GetConsoleLogger();
                Assert.That(() => messageBus.Send(request),
                    Is.SameAs(response));
            });
        }

        [Test]
        public void It_should_call_selected_IRequestProcessors_Process_method_multiple_requests()
        {
            var mocks = new MockRepository();
            var notificationProcessor = mocks.Stub<INotificationProcessor>();
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

            var requestProcessor1 = new TestRequestProcessor(responsesForProcessor1);
            var requestProcessor2 = new TestRequestProcessor(responsesForProcessor2);
            
            var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor1, requestProcessor2 }, new INotificationProcessor[] { notificationProcessor });
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
        }

        public class TestRequestProcessor : ILocalRequestProcessor
        {
            private readonly ResponsesGroup responsesGroup;

            public TestRequestProcessor(ResponsesGroup responsesGroup)
            {
                this.responsesGroup = responsesGroup;
            }

            public bool CanProcess(BaseRequest request)
            {
                return responsesGroup.ContainsKey(request);
            }

            public ResponsesGroup Process(IList<BaseRequest> requests)
            {
                return responsesGroup;
            }
        }


        [Test]
        public void It_should_run_all_the_IMessageBusSendInterceptors()
        {
            var mocks = new MockRepository();
            var notificationProcessor = mocks.Stub<INotificationProcessor>();
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
                Expect.Call(interceptor1.InterceptionPriority).Return(InterceptionPrority.High);
                Expect.Call(interceptor2.InterceptionPriority).Return(InterceptionPrority.Medium);

                interceptor1.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptSendDelegate((invocation) =>
                {
                    Assert.That(() => invocation.Requests[0].CorrelationGuid,
                        Is.EqualTo(request.CorrelationGuid));
                    invocation.Requests[0].CorrelationGuid = newCorrelationGuid;
                    invocation.Proceed();
                }));

                interceptor2.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptSendDelegate((invocation) =>
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
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor }, new INotificationProcessor[] { notificationProcessor });
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
            var notificationProcessor = mocks.Stub<INotificationProcessor>();
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
                Expect.Call(interceptor1.InterceptionPriority).Return(InterceptionPrority.Low);
                Expect.Call(interceptor2.InterceptionPriority).Return(InterceptionPrority.High);

                interceptor2.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptSendDelegate((invocation) =>
                {
                    Assert.That(() => invocation.Requests[0].CorrelationGuid,
                        Is.EqualTo(request1.CorrelationGuid));
                    Assert.That(() => invocation.Requests[1].CorrelationGuid,
                        Is.EqualTo(request2.CorrelationGuid));
                    invocation.Requests[1].CorrelationGuid = newCorrelationGuid;
                    invocation.Proceed();
                }));

                interceptor1.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptSendDelegate((invocation) =>
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
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor }, new INotificationProcessor[] { notificationProcessor });
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
            var notificationProcessor = mocks.Stub<INotificationProcessor>();
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
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor }, new INotificationProcessor[] { notificationProcessor });
                messageBus.Logger = GetConsoleLogger();
                var callbackThreadId = 0;
                messageBus.SendAsync(request).Register(r =>
                {
                    Assert.AreSame(r, response);
                    callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                });
                Thread.Sleep(500);
                Assert.That(() => callbackThreadId,
                    Is.Not.EqualTo(Thread.CurrentThread.ManagedThreadId));
            });
        }

        [Test]
        public void It_should_be_able_to_handle_exceptions_asynchronously()
        {
            var mocks = new MockRepository();
            var notificationProcessor = mocks.Stub<INotificationProcessor>();
            var request = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request };

            var requestProcessor = mocks.StrictMock<IRequestProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestProcessor.CanProcess(request)).Return(true);
                Expect.Call(requestProcessor.Process(requests)).Throw(new Exception("Test exception"));
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor }, new INotificationProcessor[] { notificationProcessor });
                messageBus.Logger = GetConsoleLogger();
                var callbackThreadId = 0;
                messageBus.SendAsync(request).Register(r =>
                {
                    Assert.Fail();
                },
                e =>
                {
                    Assert.That(() => e.ToString(),
                        Contains.Substring("Test exception"));
                    callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                });
                Thread.Sleep(500);
                Assert.That(() => callbackThreadId,
                    Is.Not.EqualTo(Thread.CurrentThread.ManagedThreadId));
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_notifying_without_INotificationProcessor()
        {
            var mocks = new MockRepository();
            var requestProcessor = mocks.StrictMock<IRequestProcessor>();
            var notification = mocks.Stub<Notification>();
            With.Mocks(mocks).Expecting(() =>
            {
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor }, null);
                Assert.That(() => messageBus.Notify(notification),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains("INotificationProcessor"));

                messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor }, new INotificationProcessor[] { });
                Assert.That(() => messageBus.Notify(notification),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains("INotificationProcessor"));
            });
        }

        [Test]
        public void It_should_use_all_the_INotificationProcessor_to_process_notifications()
        {
            var mocks = new MockRepository();
            var requestProcessor = mocks.StrictMock<IRequestProcessor>();

            var notificationProcessor1 = new TestNotificationProcessor();
            var notificationProcessor2 = new TestNotificationProcessor();

            var notification1 = mocks.Stub<Notification>();
            var notification2 = mocks.Stub<Notification>();

            With.Mocks(mocks).Expecting(() =>
            {
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor }, new INotificationProcessor[] { notificationProcessor1, notificationProcessor2 });
                messageBus.Logger = GetConsoleLogger();

                messageBus.Notify(notification1);
                Thread.Sleep(200);
                Assert.That(() => notificationProcessor1.Notifications,
                    Is.EquivalentTo(new[] { notification1 }));
                Assert.That(() => notificationProcessor2.Notifications,
                    Is.EquivalentTo(new[] { notification1 }));

                messageBus.Notify(notification1, notification2);
                Thread.Sleep(200);
                Assert.That(() => notificationProcessor1.Notifications,
                    Is.EquivalentTo(new[] { notification1, notification2 }));
                Assert.That(() => notificationProcessor2.Notifications,
                    Is.EquivalentTo(new[] { notification1, notification2 }));
            });
        }

        public class TestNotificationProcessor : INotificationProcessor
        {
            public Notification[] Notifications { get; private set; }

            public void Process(Notification[] notifications)
            {
                Notifications = notifications;
            }
        }


        [Test]
        public void It_should_run_all_the_IMessageBusNotifyInterceptors()
        {
            var mocks = new MockRepository();
            var requestProcessor = mocks.StrictMock<IRequestProcessor>();

            var notificationProcessor = mocks.Stub<INotificationProcessor>();

            var notification1 = mocks.Stub<Notification>();
            var notification2 = mocks.Stub<Notification>();

            var interceptor1 = mocks.StrictMock<IMessageBusNotifyInterceptor>();
            var interceptor2 = mocks.StrictMock<IMessageBusNotifyInterceptor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(interceptor1.InterceptionPriority).Return(InterceptionPrority.Low);
                Expect.Call(interceptor2.InterceptionPriority).Return(InterceptionPrority.High);

                interceptor1.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptNotifyDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                interceptor2.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptNotifyDelegate((invocation) =>
                {
                    invocation.Notifications.Remove(notification2);
                    invocation.Proceed();
                }));

                notificationProcessor.Process(new Notification[] { notification1 });
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IRequestProcessor[] { requestProcessor }, new INotificationProcessor[] { notificationProcessor });
                messageBus.Logger = GetConsoleLogger();
                messageBus.MessageBusNotifyInterceptors = new IMessageBusNotifyInterceptor[] { interceptor1, interceptor2 };

                messageBus.Notify(notification1, notification2);
            });
        }

        public delegate void InterceptSendDelegate(IColomboSendInvocation invocation);
        public delegate void InterceptNotifyDelegate(IColomboNotifyInvocation invocation);
        public delegate ResponsesGroup ProcessDelegate(IList<BaseRequest> requests);

        public class TestRequestForSendAction : SideEffectFreeRequest<TestResponse>
        {
            public string Name { get; set; }
        }
    }
}
