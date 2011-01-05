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
                var processor = new LocalRequestProcessor(requestHandlerFactory);
                processor.Logger = GetConsoleLogger();
                Assert.That(() => processor.CanSend(request),
                    Is.False);
            });
        }

        [Test]
        public void It_should_ensure_that_IRequestHandlerFactory_returns_a_handler_in_Create()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
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
            var request1 = mocks.Stub<Request<TestResponse>>();
            var request2 = mocks.Stub<Request<TestResponse>>();
            var requests = new List<BaseRequest> { request1, request2 };
            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var requestHandlerFactory = mocks.StrictMock<IRequestHandlerFactory>();
            var requestHandler1 = mocks.StrictMock<IRequestHandler>();
            var requestHandler2 = mocks.StrictMock<IRequestHandler>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request1)).Return(requestHandler1);
                Expect.Call(requestHandlerFactory.CreateRequestHandlerFor(request2)).Return(requestHandler2);
                Expect.Call(requestHandler1.Handle(request1)).Return(response1);
                Expect.Call(requestHandler2.Handle(request2)).Return(response2);
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
    }
}
