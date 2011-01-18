using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Colombo;
using Colombo.Facilities;
using Colombo.TestSupport;
using NUnit.Framework;

namespace Hosted.Tests
{
    public class BaseTest
    {
        protected IWindsorContainer Container { get; set; }

        protected IStubMessageBus StubMessageBus { get; set; }

        protected IMessageBus MessageBus { get; set; }

        [SetUp]
        public void SetUp()
        {
            Container = new WindsorContainer();
            Container.AddFacility<ColomboFacility>(f => f.TestSupportMode());

            StubMessageBus = Container.Resolve<IStubMessageBus>();
            MessageBus = Container.Resolve<IMessageBus>();
        }
    }
}
