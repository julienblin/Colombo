using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Colombo.Wcf
{
    /// <summary>
    /// Components that create <see cref="IWcfColomboService"/> channels.
    /// </summary>
    [ContractClass(typeof(Contracts.WcfServiceFactoryContract))]
    public interface IWcfColomboServiceFactory
    {
        bool CanCreateChannelForRequestGroup(string name);
        string GetAddressForRequestGroup(string name);
        IWcfColomboService CreateChannel(string name);
        IEnumerable<IWcfColomboService> CreateChannelsForAllEndPoints();
    }
}
