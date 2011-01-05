using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Castle.Core.Logging;
using Castle.Facilities.Logging;

namespace Colombo.Tests
{
    public class BaseTest
    {
        private IWindsorContainer container;

        protected ILogger GetConsoleLogger()
        {
            if (container == null)
            {
                container = new WindsorContainer();
                container.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Console));
            }
            return container.Resolve<ILogger>();
        }
    }
}
