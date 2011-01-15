using System.Collections.Generic;
using Castle.Windsor;

namespace Colombo.Host
{
    public interface IWantToCreateServiceHosts
    {
        IEnumerable<System.ServiceModel.ServiceHost> CreateServiceHosts(IWindsorContainer container);
    }
}
