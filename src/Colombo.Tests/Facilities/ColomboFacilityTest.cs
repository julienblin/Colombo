using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Castle.Windsor;
using Colombo.Facilities;
using Colombo.Wcf;
using System.Reflection;
using Castle.MicroKernel;
using Colombo.Interceptors;

namespace Colombo.Tests.Facilities
{
    [TestFixture]
    public class ColomboFacilityTest
    {
        [Test]
        public void It_should_register_necessaries_components()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(() => container.Resolve<IMessageBus>(),
                Is.Not.Null);
            Assert.That(() => container.Resolve<IRequestProcessor>(),
                Is.Not.Null);
            Assert.That(() => container.Resolve<ILocalRequestProcessor>(),
                Is.Not.Null);
            Assert.That(() => container.Resolve<IRequestHandlerFactory>(),
                Is.Not.Null);

            Assert.That(() => container.Resolve<IStatefulMessageBus>(),
                Is.Not.Null);
            Assert.That(() => container.Resolve<IStatefulMessageBus>(),
                Is.Not.SameAs(container.Resolve<IStatefulMessageBus>()));
        }

        [Test]
        public void It_should_position_the_kernel_in_the_wcf_static_field()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            var kernelStaticProperty = typeof(WcfService).GetProperty("Kernel", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(() => kernelStaticProperty.GetValue(null, null),
                Is.SameAs(container.Kernel));
        }

        [Test]
        public void It_should_register_necessaries_components_with_ClientOnly_option()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f =>
            {
                f.ClientOnly();
            });

            Assert.That(() => container.Resolve<IMessageBus>(),
                Is.Not.Null);

            Assert.That(() => container.Resolve<IStatefulMessageBus>(),
                Is.Not.Null);
            Assert.That(() => container.Resolve<IStatefulMessageBus>(),
                Is.Not.SameAs(container.Resolve<IStatefulMessageBus>()));

            IRequestProcessor[] messageProcessors = container.ResolveAll<IRequestProcessor>();
            Assert.That(() => messageProcessors.Length,
                Is.AtLeast(1));
            foreach (var processor in messageProcessors)
            {
                Assert.That(() => processor,
                Is.Not.AssignableFrom<ILocalRequestProcessor>());
            }

            Assert.That(() => container.Resolve<ILocalRequestProcessor>(),
                Throws.Exception.TypeOf<ComponentNotFoundException>());
            Assert.That(() => container.Resolve<IRequestHandlerFactory>(),
                Throws.Exception.TypeOf<ComponentNotFoundException>());
        }

        [Test]
        public void It_should_position_value_for_AllowMultipleFutureSendBatches()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(() => container.Resolve<IStatefulMessageBus>().MaxAllowedNumberOfSend,
                Is.EqualTo(ColomboFacility.DefaultMaxAllowedNumberOfSendForStatefulMessageBus));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.MaxAllowedNumberOfSendForStatefulMessageBus(50));

            Assert.That(() => container.Resolve<IStatefulMessageBus>().MaxAllowedNumberOfSend,
                Is.EqualTo(50));
        }

        [Test]
        public void It_should_register_TransactionScopeHandleInterceptor()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is TransactionScopeHandleInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f =>
            {
                f.DoNotHandleInsideTransactionScope();
            });
            Assert.That(!container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is TransactionScopeHandleInterceptor));
        }
    }
}
