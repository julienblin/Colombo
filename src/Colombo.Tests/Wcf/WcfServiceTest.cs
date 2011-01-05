using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Wcf;
using System.Reflection;
using Rhino.Mocks;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class WcfServiceTest : BaseTest
    {
        [SetUp]
        public void SetUp()
        {
            // Hack to reset static property value...
            var kernelStaticProperty = typeof(WcfService).GetProperty("Kernel", BindingFlags.Static | BindingFlags.NonPublic);
            kernelStaticProperty.SetValue(null, null, null);
        }

        [Test]
        public void It_should_raise_an_exception_when_registering_a_null_kernel()
        {
            Assert.That(() => WcfService.RegisterKernel(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("kernel"));
        }

        [Test]
        public void It_should_raise_an_exception_when_processing_with_a_null_kernel()
        {
            var mocks = new MockRepository();
            var request1 = mocks.Stub<Request<TestResponse>>();
            var request2 = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request1, request2 };
            var service = new WcfService();

            Assert.That(() => service.Process(requests),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("Kernel"));
        }

        [Test]
        public void It_should_throw_an_exception_when_not_finding_a_ILocalRequestProcessor()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request };
            var service = new WcfService();

            var kernel = new DefaultKernel();

            WcfService.RegisterKernel(kernel);

            Assert.That(() => service.Process(requests),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("ILocalMessageProcessor"));
        }

        [Test]
        public void It_should_throw_an_exception_when_the_ILocalRequestProcessor_cant_send()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request };
            var processor = mocks.StrictMock<ILocalRequestProcessor>();
            var service = new WcfService();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<ILocalRequestProcessor>().Instance(processor)
            );
            WcfService.RegisterKernel(kernel);

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(processor.CanSend(request)).Return(false).Repeat.Twice();
            }).Verify(() =>
            {
                Assert.That(() => service.Process(requests),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("locally"));
            });
        }

        [Test]
        public void It_should_use_the_ILocalMessageProcessor_to_process_the_requests()
        {
            var mocks = new MockRepository();
            var request1 = mocks.Stub<Request<TestResponse>>();
            var request2 = mocks.Stub<Request<TestResponse>>();
            var requests = new BaseRequest[] { request1, request2 };

            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var responsesGroup = new ResponsesGroup
            {
                { request1, response1 },
                { request2, response2 }
            };

            var processor = mocks.StrictMock<ILocalRequestProcessor>();
            var service = new WcfService();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<ILocalRequestProcessor>().Instance(processor)
            );
            WcfService.RegisterKernel(kernel);

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(processor.CanSend(request1)).Return(true);
                Expect.Call(processor.CanSend(request2)).Return(true);
                Expect.Call(processor.Process(null)).IgnoreArguments().Return(responsesGroup);
            }).Verify(() =>
            {
                var responses = service.Process(requests);
                Assert.That(() => responses[0],
                    Is.SameAs(response1));
                Assert.That(() => responses[1],
                    Is.SameAs(response2));
            });
        }
    }
}
