using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Impl;
using Rhino.Mocks;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;

namespace Colombo.Tests
{
    [TestFixture]
    public class KernelRequestHandlerFactoryTest
    {
        [Test]
        public void It_Should_Ensure_That_It_Has_A_IKernel()
        {
            Assert.That(() => new KernelRequestHandlerFactory(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("kernel"));
        }

        [Test]
        public void It_Should_Respond_To_CanCreateRequestHandlerFor_Depending_On_The_Kernel()
        {
            var mocks = new MockRepository();
            var requestHandler1 = mocks.Stub<IRequestHandler<Request1, TestResponse>>();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<IRequestHandler<Request1, TestResponse>>().Instance(requestHandler1)
            );

            var factory = new KernelRequestHandlerFactory(kernel);
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
        public void It_Should_Use_The_Kernel_To_Create_IRequestHandlers()
        {
            var mocks = new MockRepository();
            var requestHandler1 = mocks.Stub<IRequestHandler<Request1, TestResponse>>();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<IRequestHandler<Request1, TestResponse>>().Instance(requestHandler1)
            );

            var factory = new KernelRequestHandlerFactory(kernel);
            Assert.That(() => factory.CreateRequestHandlerFor(new Request1()),
                Is.SameAs(requestHandler1));
        }

        [Test]
        public void It_Should_Return_Null_When_No_IRequestHandlers_Can_Be_Created()
        {
            var mocks = new MockRepository();

            var kernel = new DefaultKernel();

            var factory = new KernelRequestHandlerFactory(kernel);
            Assert.That(() => factory.CreateRequestHandlerFor(new Request1()),
                Is.Null);
        }
    }
}
