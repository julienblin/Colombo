using System.Collections.Generic;
using System.ServiceModel;
using Castle.Windsor;

namespace Colombo.Host
{
    /// <summary>
    /// Allow a <see cref="IAmAnEndpoint"/> component to create <see cref="ServiceHost"/>.
    /// </summary>
    public interface IWantToCreateServiceHosts
    {
        /// <summary>
        /// Create the <see cref="ServiceHost"/>. Do not open them, they will be opened by Colombo.Host infrastructure.
        /// </summary>
        IEnumerable<System.ServiceModel.ServiceHost> CreateServiceHosts(IWindsorContainer container);
    }
}
