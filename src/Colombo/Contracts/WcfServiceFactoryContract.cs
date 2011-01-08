using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Colombo.Wcf;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IWcfServiceFactory))]
    public abstract class WcfServiceFactoryContract : IWcfServiceFactory
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

        public IWcfService CreateChannel(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            throw new NotImplementedException();
        }

        public IEnumerable<IWcfService> CreateChannelsForAllEndPoints()
        {
            throw new NotImplementedException();
        }
    }
}
