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
        public void It_should_ensure_that_it_has_a_IRequestHandlerFactory()
        {
            Assert.That(() => new LocalMessageProcessor(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("requestHandlerFactory"));
        }

        [Test]
        public void It_should_rely_on_IRequestHandlerFactory_for_CanSend()
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
        public void It_should_ensure_that_IRequestHandlerFactory_returns_a_handler_in_Create()
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
        public void It_should_use_the_RequestHandler_that_IRequestHandlerFactory_returns()
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
        public void It_should_run_all_the_IMessageBusSendInterceptors()
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

                requestInterceptor1.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                requestInterceptor2.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                Expect.Call(requestHandler.Handle(request)).Return(response);
                requestHandlerFactory.DisposeRequestHandler(requestHandler);
            }).Verify(() =>
            {
                var processor = new LocalMessageProcessor(requestHandlerFactory);
                processor.RequestHandlerInterceptor = new IRequestHandlerInterceptor[] { requestInterceptor1, requestInterceptor2 };
                Assert.That(() => processor.Send(request),
                    Is.SameAs(response));
            });
        }

        public delegate void InterceptDelegate(IColomboInvocation invocation);

        [Test]
        public void It_should_reorder_IRequestHandlerInterceptor_accordingly()
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

                requestInterceptor2.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                requestInterceptor3.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                requestInterceptor1.Intercept(null);
                LastCall.IgnoreArguments().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                Expect.Call(requestHandler.Handle(request)).Return(response);
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
