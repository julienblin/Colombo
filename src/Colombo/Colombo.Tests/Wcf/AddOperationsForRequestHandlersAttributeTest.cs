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
