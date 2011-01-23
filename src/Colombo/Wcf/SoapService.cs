namespace Colombo.Wcf
{
    /// <summary>
    /// Implementation of the <see cref="ISoapService"/> service.
    /// Exists as a placeholder for WCF infrastructure.
    /// All the work is done through <see cref="RequestProcessorOperationInvoker"/>
    /// </summary>
    public class SoapService : ISoapService
    {
        /// <summary>
        /// This operation is mandatory, otherwise WCF will not expose an empty interface as a contract.
        /// </summary>
        public void DummyOperationForWCF()
        {
        }
    }
}
