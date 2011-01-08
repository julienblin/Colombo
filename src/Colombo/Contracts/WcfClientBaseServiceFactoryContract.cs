using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Colombo.Wcf;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IWcfServiceFactory))]
    public abstract class WcfClientBaseServiceFactoryContract : IWcfServiceFactory
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

        public IWcfService CreateClientBase(string name)
        {
            Contract.Requires<ArgumentNullException>(name != null, "name");
            throw new NotImplementedException();
        }
    }
}
