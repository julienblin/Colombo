using Castle.Windsor;

namespace Colombo.Host
{
    /// <summary>
    /// Allow a <see cref="IAmAnEndpoint"/> component to create the <see cref="IWindsorContainer"/>.
    /// </summary>
    public interface IWantToCreateTheContainer
    {
        /// <summary>
        /// Create the <see cref="IWindsorContainer"/>
        /// </summary>
        IWindsorContainer CreateContainer();
    }
}
