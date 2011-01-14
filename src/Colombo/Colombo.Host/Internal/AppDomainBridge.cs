using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Colombo.Host.Internal
{
    internal class AppDomainBridge
    {
        private readonly DirectoryInfo baseDirectory;
        private readonly Type configureThisEndpointType;

        private AppDomain appDomain;
        private HostService hostService;

        public AppDomainBridge(DirectoryInfo baseDirectory, Type configureThisEndpointType)
        {
            this.baseDirectory = baseDirectory;
            this.configureThisEndpointType = configureThisEndpointType;
        }

        internal void Start()
        {
            var appDomainSetup = new AppDomainSetup();
            appDomainSetup.ApplicationBase = baseDirectory.FullName;
            appDomainSetup.ConfigurationFile = string.Concat(configureThisEndpointType.Assembly.Location, ".config");

            appDomain = AppDomain.CreateDomain("HostedAppDomain", null, appDomainSetup);
            hostService = (HostService)appDomain.CreateInstanceAndUnwrap(typeof(HostService).Assembly.FullName, typeof(HostService).FullName);
            hostService.BaseDirectory = baseDirectory;
            hostService.ConfigureThisEndpointType = configureThisEndpointType;

            hostService.Start();
        }

        internal void Stop()
        {
            if (hostService != null)
                hostService.Stop();

            if (appDomain != null)
                AppDomain.Unload(appDomain);
        }
    }
}
