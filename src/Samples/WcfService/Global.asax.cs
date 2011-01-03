using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Castle.Windsor;
using Colombo.Facilities;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;

namespace Colombo.Samples.WcfService
{
    public class Global : System.Web.HttpApplication
    {
        IWindsorContainer container;

        protected void Application_Start(object sender, EventArgs e)
        {
            container = new WindsorContainer();
            container.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Trace));
            container.AddFacility<ColomboFacility>();

            container.Register(
                AllTypes.FromThisAssembly().BasedOn<IRequestHandler>()
                    .WithService.AllInterfaces()
                    .Configure(c => c.LifeStyle.Transient)
            );
        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}