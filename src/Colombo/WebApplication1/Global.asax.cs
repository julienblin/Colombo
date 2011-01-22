using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Colombo;
using Colombo.Facilities;
using Colombo.Wcf;

namespace WebApplication1
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            container.Register(
                Component.For<IRequestHandler, ISideEffectFreeRequestHandler<HelloWorldRequest, HelloWorldResponse>>()
                    .ImplementedBy<HelloWorldRequestHandler>()
                    .LifeStyle.Transient,
                Component.For<IRequestHandler, IRequestHandler<CreateCandidateRequest, CreateCandidateResponse>>()
                    .ImplementedBy<CreateCandidateRequestHandler>()
                    .LifeStyle.Transient
                );
            
            WcfJsBridgeService.RegisterRequest(typeof(HelloWorldRequest));
            WcfJsBridgeService.RegisterRequest(typeof(CreateCandidateRequest));
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