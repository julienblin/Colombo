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
    }
}
