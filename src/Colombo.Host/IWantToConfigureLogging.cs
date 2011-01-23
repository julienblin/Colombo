using Castle.Windsor;

namespace Colombo.Host
{
    /// <summary>
    /// Allow a <see cref="IAmAnEndpoint"/> component to configure logging.
    /// </summary>
    public interface IWantToConfigureLogging
    {
        /// <summary>
        /// Register and configure logging infrastructure inside the <paramref name="container"/>.
        /// </summary>
        /// <param name="container"></param>
        void ConfigureLogging(IWindsorContainer container);
    }
}
