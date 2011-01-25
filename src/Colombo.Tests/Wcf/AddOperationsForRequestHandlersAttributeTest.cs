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
using System.ServiceModel.Description;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Colombo.Wcf;
using NUnit.Framework;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class AddOperationsForRequestHandlersAttributeTest
    {
        [SetUp]
        public void SetUp()
        {
            WcfServices.Kernel = null;
        }

        [Test]
        public void It_should_throw_an_exception_if_kernel_has_not_been_registered()
        {
            var addOperations = new AddOperationsForRequestHandlersAttribute();
            Assert.That(() => addOperations.AddBindingParameters(new ContractDescription("Test"), null, null),
                        Throws.Exception.TypeOf<ColomboException>()
                            .With.Message.Contains("Kernel"));
        }

        [Test]
        public void It_should_remove_wcf_dummy_operation()
        {
            var addOperations = new AddOperationsForRequestHandlersAttribute();
            var contractDescription = new ContractDescription("Test");
            contractDescription.Operations.Add(new OperationDescription("DummyOperationForWCF", new ContractDescription("DummyOperationForWCF")));

            var container = new WindsorContainer();
            WcfServices.Kernel = container.Kernel;

            addOperations.AddBindingParameters(contractDescription, null, null);

            Assert.That(() => contractDescription.Operations.Count,
                Is.EqualTo(0));
        }

        [Test]
        public void It_should_add_operations_for_every_registered_request_handler()
        {
            var addOperations = new AddOperationsForRequestHandlersAttribute();
            var contractDescription = new ContractDescription("Test");

            var container = new WindsorContainer();
            container.Register(
                Component.For<IRequestHandler>().ImplementedBy<TestRequestHandler>(),
                Component.For<IRequestHandler>().ImplementedBy<Test2RequestHandler>()
            );
            WcfServices.Kernel = container.Kernel;

            addOperations.AddBindingParameters(contractDescription, null, null);

            Assert.That(() => contractDescription.Operations.Count,
                Is.EqualTo(2));
        }

        [Test]
        public void It_should_do_nothing_with_Validate()
        {
            var addOperations = new AddOperationsForRequestHandlersAttribute();
            Assert.DoesNotThrow(() => addOperations.Validate(null, null));
        }

        [Test]
        public void It_should_do_nothing_with_ApplyClientBehavior()
        {
            var addOperations = new AddOperationsForRequestHandlersAttribute();
            Assert.DoesNotThrow(() => addOperations.ApplyClientBehavior(null, null, null));
        }

        public class TestRequest : Request<TestResponse>
        {}

        public class TestRequestHandler : RequestHandler<TestRequest, TestResponse>
        {
            protected override void Handle()
            {
                throw new NotImplementedException();
            }
        }

        public class Test2Request : Request<TestResponse>
        { }

        public class Test2RequestHandler : RequestHandler<Test2Request, TestResponse>
        {
            protected override void Handle()
            {
                throw new NotImplementedException();
            }
        }
    }
}
