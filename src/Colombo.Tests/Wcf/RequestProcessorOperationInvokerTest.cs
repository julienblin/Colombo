using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Colombo.Wcf;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class RequestProcessorOperationInvokerTest
    {
        [Test]
        public void It_should_allocate_the_correct_request_Type()
        {
            var invoker = new RequestProcessorOperationInvoker(typeof(TestRequest));
            var allocateInputs = invoker.AllocateInputs();

            Assert.That(() => allocateInputs.Length,
                Is.EqualTo(1));

            Assert.That(() => allocateInputs[0],
                Is.TypeOf<TestRequest>());
        }

        [Test]
        public void It_should_declare_itself_as_synchronous()
        {
            var invoker = new RequestProcessorOperationInvoker(typeof(TestRequest));
            Assert.That(invoker.IsSynchronous);
        }

        [Test]
        public void It_should_invoker_correct_request_handler()
        {
            var mocks = new MockRepository();
            var localProcessor = mocks.StrictMock<ILocalRequestProcessor>();

            var container = new WindsorContainer();
            container.Register(
                Component.For<ILocalRequestProcessor>().Instance(localProcessor)
            );
            WcfServices.Kernel = container.Kernel;

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(localProcessor.CanProcess(null)).IgnoreArguments().Return(true);
                Expect.Call(localProcessor.Process(null)).IgnoreArguments().Do(new ProcessDelegate(requests =>
                {
                    return new ResponsesGroup { { requests[0], new TestResponse() } };
                }));
            }).Verify(() =>
            {
                var invoker = new RequestProcessorOperationInvoker(typeof(TestRequest));
                object[] outputs;
                var response = invoker.Invoke(null, new[] {new TestRequest()}, out outputs);
                Assert.That(() => response,
                    Is.TypeOf<TestResponse>());
            });
        }

        [Test]
        public void It_should_throw_an_exception_with_async_members()
        {
            var invoker = new RequestProcessorOperationInvoker(typeof(TestRequest));

            Assert.That(() => invoker.InvokeBegin(null, null, null, null),
                Throws.Exception.TypeOf<NotImplementedException>());

            object[] outputs;
            Assert.That(() => invoker.InvokeEnd(null, out outputs, null),
                Throws.Exception.TypeOf<NotImplementedException>());
        }

        public class TestRequest : Request<TestResponse>
        { }

        public delegate ResponsesGroup ProcessDelegate(IList<BaseRequest> requests);
    }
}
