using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Wcf
{
    /// <summary>
    /// Components that create <see cref="IWcfService"/> channels.
    /// </summary>
    [ContractClass(typeof(Contracts.WcfServiceFactoryContract))]
    public interface IWcfServiceFactory
    {
        bool CanCreateChannelForRequestGroup(string name);
        string GetAddressForRequestGroup(string name);
        IWcfService CreateChannel(string name);
        IEnumerable<IWcfService> CreateChannelsForAllEndPoints();
    }
}
