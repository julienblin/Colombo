using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Configuration;
using System.Diagnostics.Contracts;
using System.Configuration;

namespace Colombo.Wcf
{
    /// <summary>
    /// <see cref="IWcfClientBaseServiceFactory"/> that creates <see cref="WcfClientBaseService"/> based on standard
    /// WCF configuration.
    /// </summary>
    public class WcfConfigClientBaseServiceFactory : IWcfClientBaseServiceFactory
    {
        public bool CanCreateClientBaseForRequestGroup(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Contract.EndContractBlock();

            return (GetChannelEndpointElement(name) != null);
        }

        public string GetAddressForRequestGroup(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Contract.EndContractBlock();

            var channelEndpointElement = GetChannelEndpointElement(name);
            if (channelEndpointElement == null)
                return null;
            return channelEndpointElement.Address.AbsoluteUri;
        }

        public WcfClientBaseService CreateClientBase(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Contract.EndContractBlock();

            try
            {
                return new WcfClientBaseService(name);
            }
            catch (InvalidOperationException ex)
            {
                throw new ColomboException(string.Format("Unable to create a WCF ClientBase. Did you create a WCF client endPoint with the name {0}?", name), ex);
            }
        }

        public ChannelEndpointElement GetChannelEndpointElement(string endPointName)
        {
            if (WcfConfigClientSection == null)
                return null;

            foreach (ChannelEndpointElement endPoint in WcfConfigClientSection.Endpoints)
            {
                if (endPoint.Name.Equals(endPointName, StringComparison.InvariantCultureIgnoreCase))
                    return endPoint;
            }

            return null;
        }

        private ClientSection clientSection = null;

        public ClientSection WcfConfigClientSection
        {
            get
            {
                if (clientSection == null)
                {
                    Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    ServiceModelSectionGroup serviceModelGroup = ServiceModelSectionGroup.GetSectionGroup(configuration);
                    if (serviceModelGroup != null)
                        clientSection = serviceModelGroup.Client;
                }
                return clientSection;
            }
        }
    }
}
