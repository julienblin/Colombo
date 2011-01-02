using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Impl;
using Rhino.Mocks;
using System.Transactions;

namespace Colombo.Tests.Impl
{
    [TestFixture]
    public class LocalMessageProcessorTest
    {
        [Test]
        public void It_Should_Ensure_That_It_Has_A_IRequestHandlerFactory()
        {
            Assert.That(() => new LocalMessageProcessor(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("requestHandlerFactory"));
        }

        [Test]
        public void It_Should_Rely_On_IRequestHandlerFactory_For_CanSend()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestHandlerFactory.CanCreateRequestHandlerFor(request)).Return(false);
            }).Verify(() =>
            {
                var processor = new LocalMessageProcessor(requestHandlerFactory);
                Assert.That(() => processor.CanSend(request),
                    Is.False);
            });
        }

        [Test]
        public void It_Should_Ensure_That_IRequestHandlerFactory_Returns_A_Handler_In_Create()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request)).Return(null);
            }).Verify(() =>
            {
                var processor = new LocalMessageProcessor(requestHandlerFactory);
                Assert.That(() => processor.Send(request),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains("requestHandler"));
            });
        }

        [Test]
        public void It_Should_Use_The_RequestHandler_That_IRequestHandlerFactory_Returns()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var response = new TestResponse();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();
            var requestHandler = mocks.StrictMock<IRequestHandler>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request)).Return(requestHandler);
                Expect.Call(requestHandler.Handle(request)).Return(response);
                requestHandlerFactory.DisposeRequestHandler(requestHandler);
            }).Verify(() =>
            {
                var processor = new LocalMessageProcessor(requestHandlerFactory);
                Assert.That(() => processor.Send(request),
                    Is.SameAs(response));
            });
        }

        [Test]
        public void It_Should_Create_A_TransactionScope_For_The_RequestHandler()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();
            var requestHandler = mocks.StrictMock<IRequestHandler>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request)).Return(requestHandler);
                Expect.Call(requestHandler.Handle(request)).Do(new HandleDelegate(r =>
                {
                    Assert.That(Transaction.Current, Is.Not.Null);
                    return new TestResponse();
                }));
                requestHandlerFactory.DisposeRequestHandler(requestHandler);
            }).Verify(() =>
            {
                var processor = new LocalMessageProcessor(requestHandlerFactory);
                Assert.That(Transaction.Current, Is.Null);
                processor.Send(request);
                Assert.That(Transaction.Current, Is.Null);
            });
        }

        delegate Response HandleDelegate(BaseRequest request);

        [Test]
        public void It_Should_Run_All_The_IRequestHandlerInterceptor_BeforeHandle_And_AfterHandle_Methods()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var response = new TestResponse();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();
            var requestHandler = mocks.StrictMock<IRequestHandler>();
            var requestInterceptor1 = mocks.StrictMock<IRequestHandlerInterceptor>();
            var requestInterceptor2 = mocks.StrictMock<IRequestHandlerInterceptor>();

            With.Mocks(mocks).ExpectingInSameOrder(() =>
            {
                Expect.Call(requestInterceptor1.InterceptionPriority).Return(InterceptorPrority.High);
                Expect.Call(requestInterceptor2.InterceptionPriority).Return(InterceptorPrority.Medium);

                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request)).Return(requestHandler);

                Expect.Call(requestInterceptor1.BeforeHandle(request)).Return(null);
                Expect.Call(requestInterceptor2.BeforeHandle(request)).Return(null);

                Expect.Call(requestHandler.Handle(request)).Return(response);

                requestInterceptor2.AfterHandle(request, response);
                requestInterceptor1.AfterHandle(request, response);

                requestHandlerFactory.DisposeRequestHandler(requestHandler);
            }).Verify(() =>
            {
                var processor = new LocalMessageProcessor(requestHandlerFactory);
                processor.RequestHandlerInterceptor = new IRequestHandlerInterceptor[] { requestInterceptor1, requestInterceptor2 };
                Assert.That(() => processor.Send(request),
                    Is.SameAs(response));
            });
        }

        [Test]
        public void It_Should_Not_Send_To_RequestHandler_If_IRequestHandlerInterceptor_BeforeHandle_Returns_Non_Null()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var response = new TestResponse();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();
            var requestHandler = mocks.StrictMock<IRequestHandler>();
            var requestInterceptor1 = mocks.StrictMock<IRequestHandlerInterceptor>();
            var requestInterceptor2 = mocks.StrictMock<IRequestHandlerInterceptor>();

            With.Mocks(mocks).ExpectingInSameOrder(() =>
            {
                Expect.Call(requestInterceptor1.InterceptionPriority).Return(InterceptorPrority.High);
                Expect.Call(requestInterceptor2.InterceptionPriority).Return(InterceptorPrority.Medium);

                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request)).Return(requestHandler);

                Expect.Call(requestInterceptor1.BeforeHandle(request)).Return(response);

                requestInterceptor2.AfterHandle(request, response);
                requestInterceptor1.AfterHandle(request, response);

                requestHandlerFactory.DisposeRequestHandler(requestHandler);
            }).Verify(() =>
            {
                var processor = new LocalMessageProcessor(requestHandlerFactory);
                processor.RequestHandlerInterceptor = new IRequestHandlerInterceptor[] { requestInterceptor1, requestInterceptor2 };
                Assert.That(() => processor.Send(request),
                    Is.SameAs(response));
            });
        }

        [Test]
        public void It_Should_Reorder_IRequestHandlerInterceptor_Accordingly()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var response = new TestResponse();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();
            var requestHandler = mocks.StrictMock<IRequestHandler>();
            var requestInterceptor1 = mocks.StrictMock<IRequestHandlerInterceptor>();
            var requestInterceptor2 = mocks.StrictMock<IRequestHandlerInterceptor>();
            var requestInterceptor3 = mocks.StrictMock<IRequestHandlerInterceptor>();

            With.Mocks(mocks).ExpectingInSameOrder(() =>
            {
                Expect.Call(requestInterceptor1.InterceptionPriority).Return(InterceptorPrority.Low);
                Expect.Call(requestInterceptor2.InterceptionPriority).Return(InterceptorPrority.High);
                Expect.Call(requestInterceptor3.InterceptionPriority).Return(InterceptorPrority.Medium);

                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request)).Return(requestHandler);

                Expect.Call(requestInterceptor2.BeforeHandle(request)).Return(null);
                Expect.Call(requestInterceptor3.BeforeHandle(request)).Return(null);
                Expect.Call(requestInterceptor1.BeforeHandle(request)).Return(null);

                Expect.Call(requestHandler.Handle(request)).Return(response);

                requestInterceptor1.AfterHandle(request, response);
                requestInterceptor3.AfterHandle(request, response);
                requestInterceptor2.AfterHandle(request, response);

                requestHandlerFactory.DisposeRequestHandler(requestHandler);
            }).Verify(() =>
            {
                var processor = new LocalMessageProcessor(requestHandlerFactory);
                processor.RequestHandlerInterceptor = new IRequestHandlerInterceptor[] { requestInterceptor1, requestInterceptor2, requestInterceptor3 };
                Assert.That(() => processor.Send(request),
                    Is.SameAs(response));
            });
        }
    }
}
