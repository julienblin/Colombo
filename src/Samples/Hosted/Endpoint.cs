using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Host;
using Castle.Windsor;
using Colombo.Facilities;

namespace Hosted
{
    public class Endpoint : IAmAnEndpoint, IWantToConfigureLogging, IWantToConfigureColombo
    {
        public void ConfigureLogging(IWindsorContainer container)
        {
            
        }

        public void ConfigureColombo(IWindsorContainer container)
        {
            container.AddFacility<ColomboFacility>(f =>
            {
                f.MonitorWithPerformanceCounters();
            });
        }
    }
}
