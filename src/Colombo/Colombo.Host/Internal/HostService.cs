using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel.Configuration;
using Castle.Core.Logging;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Colombo.Facilities;
using Colombo.Wcf;

namespace Colombo.Host.Internal
{
    internal class HostService : MarshalByRefObject,
                                 IWantToCreateTheContainer, IWantToConfigureLogging, IWantToConfigureColombo,
                                 IWantToRegisterMessageHandlers, IWantToRegisterOtherComponents, IWantToCreateServiceHosts
    {
        private IWindsorContainer hostContainer;

        private readonly List<System.ServiceModel.ServiceHost> serviceHosts = new List<System.ServiceModel.ServiceHost>();

        private ILogger logger = NullLogger.Instance;

        public DirectoryInfo BaseDirectory { get; set; }
        public Type ConfigureThisEndpointType { get; set; }

        internal IAmAnEndpoint ConfigureThisEndpoint { get; private set; }

        internal void Start()
        {
            try
            {
                ConfigureThisEndpoint = (IAmAnEndpoint)Activator.CreateInstance(ConfigureThisEndpointType);
                ConfigureContainer();

                var iWantToBeNotifiedWhenStartAndStop = ConfigureThisEndpoint as IWantToBeNotifiedWhenStartAndStop;
                if (iWantToBeNotifiedWhenStartAndStop != null)
                    iWantToBeNotifiedWhenStartAndStop.Start(hostContainer);

                var iWantToCreateServiceHosts = ConfigureThisEndpoint as IWantToCreateServiceHosts ?? this;
                serviceHosts.AddRange(iWantToCreateServiceHosts.CreateServiceHosts(hostContainer));

                foreach (var serviceHost in serviceHosts)
                    serviceHost.Open();

                hostContainer.Resolve<IMessageBus>(); // Forces container resolution. Some exception from configuration mistakes can be thrown at this point.

                logger.InfoFormat("{0} host is ready to serve incoming requests from {1}...",
                    ConfigureThisEndpoint.GetType().Assembly.GetName().Name,
                    string.Join(", ", serviceHosts.Select(h => 
                            string.Join(", ", h.Description.Endpoints.Select(x => x.Address.Uri.ToString()))
                        )
                    ));
            }
            catch (Exception ex)
            {
                logger.Error("An error occured while starting the service host.", ex);
                throw;
            }
        }

        internal void Stop()
        {
            logger.InfoFormat("{0} host is stopping...", ConfigureThisEndpoint.GetType().Assembly.GetName().Name);

            foreach (var serviceHost in serviceHosts.Where(serviceHost => serviceHost != null))
            {
                try
                {
                    serviceHost.Close();
                }
                catch
                {
                    serviceHost.Abort();
                }
            }

            var iWantToBeNotifiedWhenStartAndStop = ConfigureThisEndpoint as IWantToBeNotifiedWhenStartAndStop;
            if (iWantToBeNotifiedWhenStartAndStop != null)
                iWantToBeNotifiedWhenStartAndStop.Stop(hostContainer);

            logger.InfoFormat("{0} host has stopped.", ConfigureThisEndpoint.GetType().Assembly.GetName().Name);
        }

        private void ConfigureContainer()
        {
            var iWantToCreateTheContainer = ConfigureThisEndpoint as IWantToCreateTheContainer ?? this;
            hostContainer = iWantToCreateTheContainer.CreateContainer();

            if (hostContainer == null)
                throw new ColomboHostException(string.Format("The endpoint {0} returned a null container.", ConfigureThisEndpointType));

            var iWantToConfigureLogging = ConfigureThisEndpoint as IWantToConfigureLogging ?? this;
            iWantToConfigureLogging.ConfigureLogging(hostContainer);

            if(hostContainer.Kernel.HasComponent(typeof(ILogger)))
                logger = hostContainer.Resolve<ILogger>();

            var iWantToConfigureColombo = ConfigureThisEndpoint as IWantToConfigureColombo ?? this;
            iWantToConfigureColombo.ConfigureColombo(hostContainer);

            var iWantToRegisterMessageHandlers = ConfigureThisEndpoint as IWantToRegisterMessageHandlers ?? this;
            iWantToRegisterMessageHandlers.RegisterMessageHandlers(hostContainer);

            var iWantToRegisterOtherComponents = ConfigureThisEndpoint as IWantToRegisterOtherComponents ?? this;
            iWantToRegisterOtherComponents.RegisterOtherComponents(hostContainer);
        }

        public IWindsorContainer CreateContainer()
        {
            return new WindsorContainer();
        }

        public void ConfigureLogging(IWindsorContainer container)
        {
            container.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Console));
        }

        public void ConfigureColombo(IWindsorContainer container)
        {
            container.AddFacility<ColomboFacility>();
        }

        public void RegisterMessageHandlers(IWindsorContainer container)
        {
            container.Register(
                AllTypes.FromAssemblyInDirectory(new AssemblyFilter(BaseDirectory.FullName))
                    .BasedOn<IRequestHandler>()
                    .WithService.AllInterfaces()
                    .Configure(c => c.LifeStyle.Transient),
                AllTypes.FromAssemblyInDirectory(new AssemblyFilter(BaseDirectory.FullName))
                    .BasedOn<INotificationHandler>()
                    .WithService.AllInterfaces()
                    .Configure(c => c.LifeStyle.Transient)
            );
        }

        public void RegisterOtherComponents(IWindsorContainer container)
        {
        }

        public IEnumerable<System.ServiceModel.ServiceHost> CreateServiceHosts(IWindsorContainer container)
        {
            foreach (var contract in from ServiceElement serviceElement in WcfConfigServicesSection.Services
                                     where serviceElement.Endpoints.Count > 0
                                     select serviceElement.Endpoints[0].Contract)
            {
                switch (contract)
                {
                    case "Colombo.Wcf.IColomboService":
                        yield return new System.ServiceModel.ServiceHost(typeof(ColomboService));
                        break;
                    case "Colombo.Wcf.ISoapService":
                        yield return new System.ServiceModel.ServiceHost(typeof(SoapService));
                        break;
                    default:
                        throw new ColomboException(string.Format("Unrecognized contract {0}. You should try implementing IWantToCreateServiceHosts in your IAmAnEndpoint component to create ServiceHosts yourself.", contract));
                }
            }
        }

        private ServicesSection wcfConfigServicesSection;

        private ServicesSection WcfConfigServicesSection
        {
            get
            {
                if (wcfConfigServicesSection == null)
                {
                    var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var serviceModelGroup = ServiceModelSectionGroup.GetSectionGroup(configuration);
                    if (serviceModelGroup != null)
                        wcfConfigServicesSection = serviceModelGroup.Services;
                }
                return wcfConfigServicesSection;
            }
        }
    }
}
