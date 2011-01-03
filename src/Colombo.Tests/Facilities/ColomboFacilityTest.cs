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
using Colombo.Impl;

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
            Assert.That(() => container.Resolve<IMessageProcessor>(),
                Is.Not.Null);
            Assert.That(() => container.Resolve<ILocalMessageProcessor>(),
                Is.Not.Null);
            Assert.That(() => container.Resolve<IRequestHandlerFactory>(),
                Is.Not.Null);
            Assert.That(() => container.Resolve<IColomboConfiguration>(),
                Is.Not.Null);
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

            IMessageProcessor[] messageProcessors = container.ResolveAll<IMessageProcessor>();
            Assert.That(() => messageProcessors.Length,
                Is.AtLeast(1));
            foreach (var processor in messageProcessors)
            {
                Assert.That(() => processor,
                Is.Not.AssignableFrom<ILocalMessageProcessor>());
            }

            Assert.That(() => container.Resolve<IColomboConfiguration>(),
                Is.Not.Null);

            Assert.That(() => container.Resolve<ILocalMessageProcessor>(),
                Throws.Exception.TypeOf<ComponentNotFoundException>());
            Assert.That(() => container.Resolve<IRequestHandlerFactory>(),
                Throws.Exception.TypeOf<ComponentNotFoundException>());
        }

        [Test]
        public void It_should_register_CurrentCulture_interceptors()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is CurrentCultureSendInterceptor));
            Assert.That(container.ResolveAll<IRequestHandlerInterceptor>().Any(x => x is CurrentCultureHandlerInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f =>
            {
                f.ClientOnly();
            });
            Assert.That(container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is CurrentCultureSendInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f =>
            {
                f.DoNotManageCurrentCulture();
            });
            Assert.That(!container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is CurrentCultureSendInterceptor));
            Assert.That(!container.ResolveAll<IRequestHandlerInterceptor>().Any(x => x is CurrentCultureHandlerInterceptor));
        }

        [Test]
        public void It_should_register_TransactionScopeHandlerInterceptor()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IRequestHandlerInterceptor>().Any(x => x is TransactionScopeHandlerInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f =>
            {
                f.DoNotHandleInsideTransactionScope();
            });
            Assert.That(!container.ResolveAll<IRequestHandlerInterceptor>().Any(x => x is TransactionScopeHandlerInterceptor));
        }

        [Test]
        public void It_should_register_SLASendInterceptor()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is SLASendInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f =>
            {
                f.DoNotManageSLA();
            });
            Assert.That(!container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is SLASendInterceptor));
        }

        [Test]
        public void It_should_register_DataAnnotationInterceptors()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is DataAnnotationSendInterceptor));
            Assert.That(container.ResolveAll<IRequestHandlerInterceptor>().Any(x => x is DataAnnotationHandlerInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f =>
            {
                f.DoNotValidateDataAnnotations();
            });
            Assert.That(!container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is DataAnnotationInterceptor));
            Assert.That(!container.ResolveAll<IRequestHandlerInterceptor>().Any(x => x is DataAnnotationHandlerInterceptor));
        }

        [Test]
        public void It_should_register_EventLogColomboAlerter()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IColomboAlerter>().Any(x => x is EventLogColomboAlerter));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f =>
            {
                f.DoNotAlertInApplicationEventLog();
            });
            Assert.That(!container.ResolveAll<IColomboAlerter>().Any(x => x is EventLogColomboAlerter));
        }

        [Test]
        public void It_should_register_PerfCounterHandlerInterceptor()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(!container.ResolveAll<IRequestHandlerInterceptor>().Any(x => x is PerfCounterHandlerInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f =>
            {
                f.MonitorWithPerformanceCounter();
            });
            Assert.That(container.ResolveAll<IRequestHandlerInterceptor>().Any(x => x is PerfCounterHandlerInterceptor));
        }

        [Test]
        public void It_should_register_RequiredInContextHandlerInterceptor()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IRequestHandlerInterceptor>().Any(x => x is RequiredInContextHandlerInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f =>
            {
                f.DoNotEnforceRequiredInContext();
            });
            Assert.That(!container.ResolveAll<IRequestHandlerInterceptor>().Any(x => x is RequiredInContextHandlerInterceptor));
        }
    }
}
