using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Colombo.Wcf;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(IColomboServiceFactory))]
    public abstract class ColomboServiceFactoryContract : IColomboServiceFactory
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

        public IColomboService CreateChannel(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            throw new NotImplementedException();
        }

        public IEnumerable<IColomboService> CreateChannelsForAllEndPoints()
        {
            throw new NotImplementedException();
        }
    }
#pragma warning restore 1591
}
