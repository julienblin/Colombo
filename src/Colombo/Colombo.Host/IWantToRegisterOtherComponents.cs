using Castle.Windsor;

namespace Colombo.Host
{
    /// <summary>
    /// Allow a <see cref="IAmAnEndpoint"/> component to register other components inside the container.
    /// </summary>
    public interface IWantToRegisterOtherComponents
    {
        /// <summary>
        /// Register other components (i.e. not Colombo components or Message handlers) inside the <paramref name="container"/>.
        /// </summary>
        void RegisterOtherComponents(IWindsorContainer container);
    }
}
