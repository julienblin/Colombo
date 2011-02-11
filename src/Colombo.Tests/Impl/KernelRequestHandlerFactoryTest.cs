#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Colombo.Impl.RequestHandle;
using NUnit.Framework;
using Rhino.Mocks;

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

        public class SideEffectFreeRequest1 : SideEffectFreeRequest<TestResponse>
        {
        }

        public class TestResponse2 : Response
        {
        }

        [Test]
        public void It_should_use_the_kernel_to_create_RequestHandlers()
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
        public void It_should_use_the_kernel_to_create_SideEffectFreeRequestHandlers()
        {
            var mocks = new MockRepository();
            var requestHandler1 = mocks.Stub<ISideEffectFreeRequestHandler<SideEffectFreeRequest1, TestResponse>>();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<ISideEffectFreeRequestHandler<SideEffectFreeRequest1, TestResponse>>().Instance(requestHandler1)
            );

            var factory = new KernelRequestHandlerFactory(kernel);
            Assert.That(() => factory.CreateRequestHandlerFor(new SideEffectFreeRequest1()),
                Is.SameAs(requestHandler1));
        }

        [Test]
        public void It_should_throw_an_exception_when_no_RequestHandlers_can_be_created()
        {
            var mocks = new MockRepository();

            var kernel = new DefaultKernel();
            var request1 = new Request1();

            var factory = new KernelRequestHandlerFactory(kernel);
            Assert.That(() => factory.CreateRequestHandlerFor(request1),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains(request1.ToString()));
        }

        [Test]
        public void It_should_throw_an_exception_when_no_SideEffectFreeRequestRequestHandlers_can_be_created()
        {
            var mocks = new MockRepository();

            var kernel = new DefaultKernel();
            var request1 = new SideEffectFreeRequest1();

            var factory = new KernelRequestHandlerFactory(kernel);
            Assert.That(() => factory.CreateRequestHandlerFor(request1),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains(request1.ToString()));
        }
        

        public class TestRequest : Request<TestResponse> { }

        public class GeneralTestRequestHandler : RequestHandler<TestRequest, TestResponse>
        {
            protected override void Handle()
            {
            }
        }

        public class GeneralTestRequestHandler2 : RequestHandler<TestRequest, TestResponse>
        {
            protected override void Handle()
            {
            }
        }
    }
}
