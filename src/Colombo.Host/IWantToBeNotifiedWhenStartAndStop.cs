using Castle.Windsor;

namespace Colombo.Host
{
    /// <summary>
    /// Allow a <see cref="IAmAnEndpoint"/> component to be notified when the service starts and stop.
    /// </summary>
    public interface IWantToBeNotifiedWhenStartAndStop
    {
        /// <summary>
        /// Invoked when the service starts.
        /// </summary>
        void Start(IWindsorContainer container);

        /// <summary>
        /// Invoked when the service stops.
        /// </summary>
        void Stop(IWindsorContainer container);
    }
}
