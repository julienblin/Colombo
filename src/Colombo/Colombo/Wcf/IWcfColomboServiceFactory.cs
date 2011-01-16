using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.ServiceModel;

namespace Colombo.Wcf
{
    /// <summary>
    /// Components that create <see cref="IWcfColomboService"/> channels.
    /// </summary>
    [ContractClass(typeof(Contracts.WcfServiceFactoryContract))]
    public interface IWcfColomboServiceFactory
    {
        /// <summary>
        /// <c>true</c> if can create a channel for the group named <paramref name="name"/>, <c>false</c> otherwise.
        /// </summary>
        bool CanCreateChannelForRequestGroup(string name);

        /// <summary>
        /// Return the address of the endpoint associated with the name <paramref name="name"/>.
        /// Return null if not found.
        /// </summary>
        string GetAddressForRequestGroup(string name);

        /// <summary>
        /// Create a <see cref="IClientChannel"/> associated with the name <paramref name="name"/>.
        /// </summary>
        IWcfColomboService CreateChannel(string name);

        /// <summary>
        /// Create a <see cref="IClientChannel"/> for all the available endpoints.
        /// </summary>
        IEnumerable<IWcfColomboService> CreateChannelsForAllEndPoints();
    }
}
