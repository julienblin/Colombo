using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Configuration;
using System.Diagnostics.Contracts;
using System.Configuration;
using System.ServiceModel;
using System.Collections.Concurrent;

namespace Colombo.Wcf
{
    /// <summary>
    /// <see cref="IWcfColomboServiceFactory"/> that creates <see cref="IWcfColomboService"/> channels based on standard
    /// WCF configuration.
    /// </summary>
    public class WcfColomboServiceFactory : IWcfColomboServiceFactory
    {
        public bool CanCreateChannelForRequestGroup(string name)
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

        private ConcurrentDictionary<string, ChannelFactory<IWcfColomboService>> channelFactories = new ConcurrentDictionary<string, ChannelFactory<IWcfColomboService>>();

        public IWcfColomboService CreateChannel(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Contract.EndContractBlock();

            try
            {
                var channelFactory = channelFactories.GetOrAdd(name, (n) =>
                {
                    var channelFact = new ChannelFactory<IWcfColomboService>(n);
                    channelFact.Faulted += FactoryFaulted;
                    channelFact.Open();
                    return channelFact;
                });
                
                var channel = channelFactory.CreateChannel();
                ((IClientChannel)channel).Faulted += ChannelFaulted;
                return channel;
            }
            catch (Exception ex)
            {
                throw new ColomboException(string.Format("Unable to create a WCF Channel. Did you create a WCF client endPoint with the name {0}?", name), ex);
            }
        }

        public IEnumerable<IWcfColomboService> CreateChannelsForAllEndPoints()
        {
            foreach (ChannelEndpointElement endPoint in WcfConfigClientSection.Endpoints)
            {
                yield return CreateChannel(endPoint.Name);
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

        private ClientSection wcfConfigClientSection = null;

        public ClientSection WcfConfigClientSection
        {
            get
            {
                if (wcfConfigClientSection == null)
                {
                    var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var serviceModelGroup = ServiceModelSectionGroup.GetSectionGroup(configuration);
                    if (serviceModelGroup != null)
                        wcfConfigClientSection = serviceModelGroup.Client;
                }
                return wcfConfigClientSection;
            }
        }

        private void FactoryFaulted(object sender, EventArgs args)
        {
            ChannelFactory<IWcfColomboService> factory = (ChannelFactory<IWcfColomboService>)sender;
            try
            {
                factory.Close();
            }
            catch
            {
                factory.Abort();
            }

            ChannelFactory<IWcfColomboService> outFactory;
            channelFactories.TryRemove(factory.Endpoint.Name, out outFactory);
        }

        private void ChannelFaulted(object sender, EventArgs e)
        {
            IClientChannel channel = (IClientChannel)sender;
            try
            {
                channel.Close();
            }
            catch
            {
                channel.Abort();
            }
        }
    }
}
