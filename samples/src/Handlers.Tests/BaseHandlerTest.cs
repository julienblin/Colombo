using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Colombo.Facilities;
using Colombo.TestSupport;
using NUnit.Framework;

namespace Colombo.Samples.Handlers.Tests
{
    public abstract class BaseHandlerTest
    {
        protected IWindsorContainer container;

        [SetUp]
        public void SetUp()
        {
            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.TestSupportMode());
        }

        private IMessageBus messageBus;

        protected IMessageBus MessageBus
        {
            get { return messageBus ?? (messageBus = container.Resolve<IMessageBus>()); }
        }

        private IStubMessageBus stubMessageBus;

        protected IStubMessageBus StubMessageBus
        {
            get { return stubMessageBus ?? (stubMessageBus = container.Resolve<IStubMessageBus>()); }
        }
    }
}
