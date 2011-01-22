using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;

namespace Colombo.Wcf
{
    /// <summary>
    /// <see cref="IWcfColomboServiceFactory"/> that creates <see cref="IWcfColomboService"/> channels based on standard
    /// WCF configuration.
    /// </summary>
    public class WcfColomboServiceFactory : IWcfColomboServiceFactory
    {
        /// <summary>
        /// <c>true</c> if can create a channel for the group named <paramref name="name"/>, <c>false</c> otherwise.
        /// </summary>
        public bool CanCreateChannelForRequestGroup(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Contract.EndContractBlock();

            return (GetChannelEndpointElement(name) != null);
        }

        /// <summary>
        /// Return the address of the endpoint associated with the name <paramref name="name"/>.
        /// Return null if not found.
        /// </summary>
        public string GetAddressForRequestGroup(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Contract.EndContractBlock();

            var channelEndpointElement = GetChannelEndpointElement(name);
            return channelEndpointElement == null ? null : channelEndpointElement.Address.AbsoluteUri;
        }

        private readonly ConcurrentDictionary<string, ChannelFactory<IWcfColomboService>> channelFactories = new ConcurrentDictionary<string, ChannelFactory<IWcfColomboService>>();

        /// <summary>
        /// Create a <see cref="IClientChannel"/> associated with the name <paramref name="name"/>.
        /// </summary>
        public IWcfColomboService CreateChannel(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Contract.EndContractBlock();

            try
            {
                var channelFactory = channelFactories.GetOrAdd(name, n =>
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

        /// <summary>
        /// Create a <see cref="IClientChannel"/> for all the available endpoints.
        /// </summary>
        public IEnumerable<IWcfColomboService> CreateChannelsForAllEndPoints()
        {
            return from ChannelEndpointElement endPoint in WcfConfigClientSection.Endpoints select CreateChannel(endPoint.Name);
        }

        private ChannelEndpointElement GetChannelEndpointElement(string endPointName)
        {
            return WcfConfigClientSection == null ?
                null : WcfConfigClientSection.Endpoints.Cast<ChannelEndpointElement>().FirstOrDefault(endPoint => endPoint.Name.Equals(endPointName, StringComparison.InvariantCultureIgnoreCase));
        }

        private ClientSection wcfConfigClientSection;

        private ClientSection WcfConfigClientSection
        {
            get {
                return wcfConfigClientSection ??
                       (wcfConfigClientSection =
                        ConfigurationManager.GetSection("system.serviceModel/client") as ClientSection);
            }
        }

        private void FactoryFaulted(object sender, EventArgs args)
        {
            var factory = (ChannelFactory<IWcfColomboService>)sender;
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

        private static void ChannelFaulted(object sender, EventArgs e)
        {
            var channel = (IClientChannel)sender;
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
