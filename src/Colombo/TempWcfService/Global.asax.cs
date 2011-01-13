using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Castle.Windsor;
using Colombo.Facilities;
using Castle.MicroKernel.Registration;
using Colombo;

namespace TempWcfService
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            container.Register(
                AllTypes.FromAssemblyContaining<Global>()
                .BasedOn(typeof(IRequestHandler))
                .WithService.AllInterfaces()
            );
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}