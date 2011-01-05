using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Colombo.Impl;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;

namespace Colombo.Tests.Impl
{
    [TestFixture]
    public class KernelRequestHandlerFactoryTest : BaseTest
    {
        [Test]
        public void It_should_ensure_that_it_has_a_IKernel()
        {
            Assert.That(() => new KernelRequestHandlerFactory(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("kernel"));
        }

        [Test]
        public void It_should_respond_to_cancreaterequesthandlerfor_depending_on_the_kernel()
        {
            var mocks = new MockRepository();
            var requestHandler1 = mocks.Stub<IRequestHandler<Request1, TestResponse>>();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<IRequestHandler<Request1, TestResponse>>().Instance(requestHandler1)
            );

            var factory = new KernelRequestHandlerFactory(kernel);
            factory.Logger = GetConsoleLogger();
            Assert.That(() => factory.CanCreateRequestHandlerFor(new Request1()),
                Is.True);
            Assert.That(() => factory.CanCreateRequestHandlerFor(new Request2()),
                Is.False);
        }

        public class Request1 : Request<TestResponse>
        {
        }

        public class Request2 : Request<TestResponse2>
        {
        }

        public class TestResponse2 : Response
        {
        }

        [Test]
        public void It_should_use_the_kernel_to_create_IRequestHandlers()
        {
            var mocks = new MockRepository();
            var requestHandler1 = mocks.Stub<IRequestHandler<Request1, TestResponse>>();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<IRequestHandler<Request1, TestResponse>>().Instance(requestHandler1)
            );

            var factory = new KernelRequestHandlerFactory(kernel);
            factory.Logger = GetConsoleLogger();
            Assert.That(() => factory.CreateRequestHandlerFor(new Request1()),
                Is.SameAs(requestHandler1));
        }

        [Test]
        public void It_should_return_null_when_no_IRequestHandlers_can_be_created()
        {
            var mocks = new MockRepository();

            var kernel = new DefaultKernel();

            var factory = new KernelRequestHandlerFactory(kernel);
            factory.Logger = GetConsoleLogger();
            Assert.That(() => factory.CreateRequestHandlerFor(new Request1()),
                Is.Null);
        }
    }
}
