using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Colombo.Wcf;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(IWcfColomboServiceFactory))]
    public abstract class WcfServiceFactoryContract : IWcfColomboServiceFactory
    {
        public bool CanCreateChannelForRequestGroup(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            throw new NotImplementedException();
        }

        public string GetAddressForRequestGroup(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            throw new NotImplementedException();
        }

        public IWcfColomboService CreateChannel(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            throw new NotImplementedException();
        }

        public IEnumerable<IWcfColomboService> CreateChannelsForAllEndPoints()
        {
            throw new NotImplementedException();
        }
    }
#pragma warning restore 1591
}
