using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Colombo.Wcf;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IWcfClientBaseServiceFactory))]
    public abstract class WcfClientBaseServiceFactoryContract : IWcfClientBaseServiceFactory
    {
        public bool CanCreateClientBaseForRequestGroup(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            throw new NotImplementedException();
        }

        public string GetAddressForRequestGroup(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            throw new NotImplementedException();
        }

        public WcfClientBaseService CreateClientBase(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            throw new NotImplementedException();
        }
    }
}
