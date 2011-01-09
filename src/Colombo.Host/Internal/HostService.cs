using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Castle.Facilities.Logging;
using Colombo.Facilities;
using System.ComponentModel;
using Castle.MicroKernel.Registration;
using System.IO;
using Castle.Core.Logging;
using Colombo.Wcf;

namespace Colombo.Host.Internal
{
    internal class HostService : IWantToConfigureLogging, IWantToConfigureColombo, IWantToRegisterMessageHandlers,
                                 IWantToRegisterOtherComponents, IWantToCreateServiceHost
    {
        private readonly DirectoryInfo baseDirectory;
        private readonly IAmAnEndpoint configureThisEndpoint;
        private readonly IWindsorContainer container;

        private System.ServiceModel.ServiceHost serviceHost;

        private ILogger logger = NullLogger.Instance;

        internal HostService(DirectoryInfo baseDirectory, IAmAnEndpoint configureThisEndpoint)
        {
            this.baseDirectory = baseDirectory;
            this.configureThisEndpoint = configureThisEndpoint;
            container = new WindsorContainer();
        }

        internal void Start()
        {
            try
            {
                ConfigureContainer();

                var iWantToBeNotifiedWhenStartAndStop = configureThisEndpoint as IWantToBeNotifiedWhenStartAndStop;
                if (iWantToBeNotifiedWhenStartAndStop != null)
                    iWantToBeNotifiedWhenStartAndStop.Start(container);

                var iWantToCreateServiceHost = configureThisEndpoint as IWantToCreateServiceHost ?? this;
                serviceHost = iWantToCreateServiceHost.CreateServiceHost(container);
                serviceHost.Open();

                logger.InfoFormat("{0} host is ready to serve incoming requests from {1}...",
                    configureThisEndpoint.GetType().Assembly.GetName().Name,
                    string.Join(", ", serviceHost.Description.Endpoints.Select(x => x.Address.Uri.ToString())));
            }
            catch (Exception ex)
            {
                logger.Error("An error occured while starting the service host.", ex);
                throw;
            }
        }

        internal void Stop()
        {
            logger.InfoFormat("{0} host is stopping...", configureThisEndpoint.GetType().Assembly.GetName().Name);

            serviceHost.Close();

            var iWantToBeNotifiedWhenStartAndStop = configureThisEndpoint as IWantToBeNotifiedWhenStartAndStop;
            if (iWantToBeNotifiedWhenStartAndStop != null)
                iWantToBeNotifiedWhenStartAndStop.Stop(container);

            logger.InfoFormat("{0} host has stopped.", configureThisEndpoint.GetType().Assembly.GetName().Name);
        }

        private void ConfigureContainer()
        {
            var iWantToConfigureLogging = configureThisEndpoint as IWantToConfigureLogging ?? this;
            iWantToConfigureLogging.ConfigureLogging(container);

            if(container.Kernel.HasComponent(typeof(ILogger)))
                logger = container.Resolve<ILogger>();

            var iWantToConfigureColombo = configureThisEndpoint as IWantToConfigureColombo ?? this;
            iWantToConfigureColombo.ConfigureColombo(container);

            var iWantToRegisterMessageHandlers = configureThisEndpoint as IWantToRegisterMessageHandlers ?? this;
            iWantToRegisterMessageHandlers.RegisterMessageHandlers(container);

            var iWantToRegisterOtherComponents = configureThisEndpoint as IWantToRegisterOtherComponents ?? this;
            iWantToRegisterOtherComponents.RegisterOtherComponents(container);
        }

        public void ConfigureLogging(IWindsorContainer container)
        {
            if (HostServiceUtil.IsRunningInCommandLineMode())
                container.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Console));
            else
                container.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Log4net));
        }

        public void ConfigureColombo(IWindsorContainer container)
        {
            container.AddFacility<ColomboFacility>();
        }

        public void RegisterMessageHandlers(IWindsorContainer container)
        {
            container.Register(
                AllTypes.FromAssemblyInDirectory(new AssemblyFilter(baseDirectory.FullName))
                    .BasedOn<IRequestHandler>()
                    .WithService.AllInterfaces()
                    .Configure(c => c.LifeStyle.Transient)
            );
        }

        public void RegisterOtherComponents(IWindsorContainer container)
        {
        }

        public System.ServiceModel.ServiceHost CreateServiceHost(IWindsorContainer container)
        {
            return new System.ServiceModel.ServiceHost(typeof(WcfService));
        }
    }
}
