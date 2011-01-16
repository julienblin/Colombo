namespace Colombo.Wcf
{
    /// <summary>
    /// Implementation of the <see cref="IWcfSoapService"/> service.
    /// Exists as a placeholder for WCF infrastructure.
    /// All the work is done through <see cref="RequestProcessorOperationInvoker"/>
    /// </summary>
    public class WcfSoapService : IWcfSoapService
    {
        /// <summary>
        /// This operation is mandatory, otherwise WCF will not expose an empty interface as a contract.
        /// </summary>
        public void DummyOperationForWCF()
        {
        }
    }
}
