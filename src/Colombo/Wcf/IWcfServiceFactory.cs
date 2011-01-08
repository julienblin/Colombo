using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Wcf
{
    /// <summary>
    /// Components that create <see cref="WcfClientBaseService"/> instances.
    /// </summary>
    [ContractClass(typeof(Contracts.WcfClientBaseServiceFactoryContract))]
    public interface IWcfServiceFactory
    {
        bool CanCreateClientBaseForRequestGroup(string name);
        string GetAddressForRequestGroup(string name);
        IWcfService CreateClientBase(string name);
    }
}
