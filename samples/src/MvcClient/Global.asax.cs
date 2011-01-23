using System;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Colombo.Facilities;
using Colombo.Samples.Messages;
using Colombo.Wcf;

namespace Colombo.Samples.MvcClient
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private IWindsorContainer container;

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterRoutes(RouteTable.Routes);

            ConfigureContainer();
            ConfigureRestService();
        }

        private void ConfigureContainer()
        {
            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.ClientOnly());

            container.Register(
                AllTypes.FromThisAssembly()
                    .BasedOn<IController>()
                    .Configure(c => c.LifeStyle.Transient)
                );

            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container));
        }

        private void ConfigureRestService()
        {
            ClientRestService.RegisterRequest<HelloWorldRequest>();
        }
    }
}