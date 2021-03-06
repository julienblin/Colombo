﻿#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

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
    /// <see cref="IColomboServiceFactory"/> that creates <see cref="IColomboService"/> channels based on standard
    /// WCF configuration.
    /// </summary>
    public class ColomboServiceFactory : IColomboServiceFactory
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

        private readonly ConcurrentDictionary<string, ChannelFactory<IColomboService>> channelFactories = new ConcurrentDictionary<string, ChannelFactory<IColomboService>>();

        /// <summary>
        /// Create a <see cref="IClientChannel"/> associated with the name <paramref name="name"/>.
        /// </summary>
        public IColomboService CreateChannel(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Contract.EndContractBlock();

            try
            {
                var channelFactory = channelFactories.GetOrAdd(name, n =>
                {
                    var channelFact = new ChannelFactory<IColomboService>(n);
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
        public IEnumerable<IColomboService> CreateChannelsForAllEndPoints()
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
            var factory = (ChannelFactory<IColomboService>)sender;
            try
            {
                factory.Close();
            }
            catch
            {
                factory.Abort();
            }

            ChannelFactory<IColomboService> outFactory;
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
