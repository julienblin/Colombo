using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Impl;
using Rhino.Mocks;

namespace Colombo.Tests
{
    [TestFixture]
    public class MessageBusTest
    {
        [Test]
        public void It_Should_Ensure_That_At_Least_One_IMessageProcessor_Is_Provided()
        {
            Assert.That(() => new MessageBus(null),
                Throws.Exception.TypeOf<ArgumentException>()
                .With.Message.Contains("messageProcessors"));

            Assert.That(() => new MessageBus(new IMessageProcessor[] { }),
                Throws.Exception.TypeOf<ArgumentException>()
                .With.Message.Contains("messageProcessors"));
        }

        [Test]
        public void It_Should_Throw_An_Exception_When_No_IMessageProcessor_Can_Send()
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
                Assert.That(() => messageBus.Send<TestResponse>(request),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains(messageProcessor.GetType().Name));
            });
        }

        [Test]
        public void It_Should_Throw_An_Exception_When_Too_Many_IMessageProcessors_Can_Send()
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
        public void It_Should_Call_Selected_IMessageProcessors_Send_Method()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var response = new TestResponse();

            var messageProcessor1 = mocks.StrictMock<IMessageProcessor>();
            var messageProcessor2 = mocks.StrictMock<IMessageProcessor>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(messageProcessor1.CanSend(request)).Return(true);
                Expect.Call(messageProcessor2.CanSend(request)).Return(false);
                Expect.Call(messageProcessor1.Send(request)).Return(response);
            }).Verify(() =>
            {
                var messageBus = new MessageBus(new IMessageProcessor[] { messageProcessor1, messageProcessor2 });
                Assert.That(() => messageBus.Send<TestResponse>(request),
                    Is.SameAs(response));
            });
        }

        [Test]
        public void It_Should_Run_All_The_IMessageBusSendInterceptor_BeforeSend_And_AfterMessageProcessorSend_Methods()
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

                Expect.Call(sendInterceptor1.BeforeSend<TestResponse>(request)).Return(null);
                Expect.Call(sendInterceptor2.BeforeSend<TestResponse>(request)).Return(null);

                Expect.Call(messageProcessor.Send(request)).Return(response);

                sendInterceptor2.AfterMessageProcessorSend(request, response);
                sendInterceptor1.AfterMessageProcessorSend(request, response);
            }).Verify(() =>
            {
                messageBus.MessageBusSendInterceptors = new IMessageBusSendInterceptor[] { sendInterceptor1, sendInterceptor2 };
                messageBus.Send<TestResponse>(request);
            });
        }

        [Test]
        public void It_Should_Not_Send_To_MessageProcessor_If_IMessageBusSendInterceptor_BeforeSend_Returns_Non_Null()
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

                Expect.Call(sendInterceptor1.BeforeSend<TestResponse>(request)).Return(response);

                sendInterceptor2.AfterMessageProcessorSend(request, response);
                sendInterceptor1.AfterMessageProcessorSend(request, response);
            }).Verify(() =>
            {
                messageBus.MessageBusSendInterceptors = new IMessageBusSendInterceptor[] { sendInterceptor1, sendInterceptor2 };
                messageBus.Send<TestResponse>(request);
            });
        }

        [Test]
        public void It_Should_Reorder_IMessageBusSendInterceptor_Accordingly()
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

                Expect.Call(sendInterceptor2.BeforeSend<TestResponse>(request)).Return(null);
                Expect.Call(sendInterceptor3.BeforeSend<TestResponse>(request)).Return(null);
                Expect.Call(sendInterceptor1.BeforeSend<TestResponse>(request)).Return(null);

                Expect.Call(messageProcessor.Send(request)).Return(response);

                sendInterceptor1.AfterMessageProcessorSend(request, response);
                sendInterceptor3.AfterMessageProcessorSend(request, response);
                sendInterceptor2.AfterMessageProcessorSend(request, response);
            }).Verify(() =>
            {
                messageBus.MessageBusSendInterceptors = new IMessageBusSendInterceptor[] { sendInterceptor1, sendInterceptor2, sendInterceptor3 };
                messageBus.Send<TestResponse>(request);
            });
        }
    }
}
