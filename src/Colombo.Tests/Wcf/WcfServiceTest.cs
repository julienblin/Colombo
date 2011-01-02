using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Wcf;
using Rhino.Mocks;
using Castle.MicroKernel;
using System.Reflection;
using Castle.MicroKernel.Registration;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class WcfServiceTest
    {
        [SetUp]
        public void SetUp()
        {
            // Hack to reset static property value...
            var registerKernelMethod = typeof(WcfService).GetProperty("Kernel", BindingFlags.Static | BindingFlags.NonPublic);
            registerKernelMethod.SetValue(null, null, null);
        }

        [Test]
        public void It_should_raise_an_exception_when_registering_a_null_kernel()
        {
            Assert.That(() => WcfService.RegisterKernel(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("kernel"));
        }

        [Test]
        public void It_should_raise_an_exception_when_sending_with_a_null_kernel()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var service = new WcfService();

            Assert.That(() => service.Send(request),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("Kernel"));
        }

        [Test]
        public void It_should_throw_an_exception_when_not_finding_a_ILocalMessageProcessor()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var service = new WcfService();

            var kernel = new DefaultKernel();

            WcfService.RegisterKernel(kernel);

            Assert.That(() => service.Send(request),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("ILocalMessageProcessor"));
        }

        [Test]
        public void It_should_throw_an_exception_when_the_ILocalMessageProcessor_cant_send()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var processor = mocks.StrictMock<ILocalMessageProcessor>();
            var service = new WcfService();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<ILocalMessageProcessor>().Instance(processor)
            );
            WcfService.RegisterKernel(kernel);

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(processor.CanSend(request)).Return(false);
            }).Verify(() =>
            {
                Assert.That(() => service.Send(request),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("locally"));
            });
        }

        [Test]
        public void It_should_use_the_ILocalMessageProcessor_to_send_the_request()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var response = new TestResponse();
            var processor = mocks.StrictMock<ILocalMessageProcessor>();
            var service = new WcfService();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<ILocalMessageProcessor>().Instance(processor)
            );
            WcfService.RegisterKernel(kernel);

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(processor.CanSend(request)).Return(true);
                Expect.Call(processor.Send(request)).Return(response);
            }).Verify(() =>
            {
                Assert.That(() => service.Send(request),
                    Is.SameAs(response));
            });
        }
    }
}
