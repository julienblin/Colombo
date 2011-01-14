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
using System.Configuration;

namespace Colombo.Host.Internal
{
    internal class HostService : MarshalByRefObject,
                                 IWantToCreateTheContainer, IWantToConfigureLogging, IWantToConfigureColombo,
                                 IWantToRegisterMessageHandlers, IWantToRegisterOtherComponents, IWantToCreateServiceHosts
    {
        private IAmAnEndpoint configureThisEndpoint;
        private IWindsorContainer container;

        private List<System.ServiceModel.ServiceHost> serviceHosts = new List<System.ServiceModel.ServiceHost>();

        private ILogger logger = NullLogger.Instance;

        public HostService()
        {
        }

        public DirectoryInfo BaseDirectory { get; set; }
        public Type ConfigureThisEndpointType { get; set; }

        internal void Start()
        {
            try
            {
                configureThisEndpoint = (IAmAnEndpoint)Activator.CreateInstance(ConfigureThisEndpointType);
                ConfigureContainer();

                var iWantToBeNotifiedWhenStartAndStop = configureThisEndpoint as IWantToBeNotifiedWhenStartAndStop;
                if (iWantToBeNotifiedWhenStartAndStop != null)
                    iWantToBeNotifiedWhenStartAndStop.Start(container);

                var iWantToCreateServiceHosts = configureThisEndpoint as IWantToCreateServiceHosts ?? this;
                serviceHosts.AddRange(iWantToCreateServiceHosts.CreateServiceHosts(container));

                foreach (var serviceHost in serviceHosts)
                    serviceHost.Open();

                container.Resolve<IMessageBus>(); // Forces container resolution. Some exception from configuration mistakes can be thrown at this point.

                logger.InfoFormat("{0} host is ready to serve incoming requests from {1}...",
                    configureThisEndpoint.GetType().Assembly.GetName().Name,
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
            logger.InfoFormat("{0} host is stopping...", configureThisEndpoint.GetType().Assembly.GetName().Name);

            foreach (var serviceHost in serviceHosts)
            {
                if (serviceHost != null)
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
            }

            var iWantToBeNotifiedWhenStartAndStop = configureThisEndpoint as IWantToBeNotifiedWhenStartAndStop;
            if (iWantToBeNotifiedWhenStartAndStop != null)
                iWantToBeNotifiedWhenStartAndStop.Stop(container);

            logger.InfoFormat("{0} host has stopped.", configureThisEndpoint.GetType().Assembly.GetName().Name);
        }

        private void ConfigureContainer()
        {
            var iWantToCreateTheContainer = configureThisEndpoint as IWantToCreateTheContainer ?? this;
            container = iWantToCreateTheContainer.CreateContainer();

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
            foreach (System.ServiceModel.Configuration.ServiceElement serviceElement in WcfConfigServicesSection.Services)
            {
                if (serviceElement.Endpoints.Count > 0)
                {
                    var contract = serviceElement.Endpoints[0].Contract;
                    switch (contract)
                    {
                        case "Colombo.Wcf.IWcfColomboService":
                            yield return new System.ServiceModel.ServiceHost(typeof(WcfColomboService));
                            break;
                        case "Colombo.Wcf.IWcfSoapService":
                            yield return new System.ServiceModel.ServiceHost(typeof(WcfSoapService));
                            break;
                        default:
                            throw new ColomboException(string.Format("Unrecognized contract {0}. You should try implementing IWantToCreateServiceHosts in your IAmAnEndpoint component to create ServiceHosts yourself.", contract));
                    }
                }
            }
        }

        private System.ServiceModel.Configuration.ServicesSection wcfConfigServicesSection = null;

        public System.ServiceModel.Configuration.ServicesSection WcfConfigServicesSection
        {
            get
            {
                if (wcfConfigServicesSection == null)
                {
                    var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var serviceModelGroup = System.ServiceModel.Configuration.ServiceModelSectionGroup.GetSectionGroup(configuration);
                    if (serviceModelGroup != null)
                        wcfConfigServicesSection = serviceModelGroup.Services;
                }
                return wcfConfigServicesSection;
            }
        }
    }
}
