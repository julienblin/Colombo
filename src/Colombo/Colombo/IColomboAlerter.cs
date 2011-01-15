using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Represents an alerter that will be notified in case of alerts.
    /// You must register all the alerter through the container.
    /// </summary>
    [ContractClass(typeof(Contracts.ColomboAlerterContract))]
    public interface IColomboAlerter
    {
        /// <summary>
        /// Called when an alert has been raised.
        /// </summary>
        void Alert(IColomboAlert alert);
    }
}
