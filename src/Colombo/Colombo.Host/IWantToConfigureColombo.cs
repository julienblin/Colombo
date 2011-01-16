using Castle.Windsor;
using Colombo.Facilities;

namespace Colombo.Host
{
    /// <summary>
    /// Allow a <see cref="IAmAnEndpoint"/> component to customization the registration of Colombo components
    /// inside and endpoint. Could use <see cref="ColomboFacility"/> or custome manual registration
    /// </summary>
    public interface IWantToConfigureColombo
    {
        /// <summary>
        /// Register Colombo components inside the <paramref name="container"/>.
        /// </summary>
        void ConfigureColombo(IWindsorContainer container);
    }
}
