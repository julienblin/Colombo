using Castle.Windsor;

namespace Colombo.Host
{
    /// <summary>
    /// Allow a <see cref="IAmAnEndpoint"/> component to customize the registration of the message handlers.
    /// </summary>
    public interface IWantToRegisterMessageHandlers
    {
        /// <summary>
        /// Register message handlers inside the <paramref name="container"/>.
        /// </summary>
        void RegisterMessageHandlers(IWindsorContainer container);
    }
}
