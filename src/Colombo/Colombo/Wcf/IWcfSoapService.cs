using System.Runtime.Serialization;
using System.ServiceModel;

namespace Colombo.Wcf
{
    /// <summary>
    /// Service that can be exposed to maximise interoperability.
    /// Exposes each request handler as an individual method, and ensures that the <see cref="DataContractSerializer"/> is used.
    /// </summary>
    [ServiceContract(Namespace = WcfServices.Namespace)]
    [AddOperationsForRequestHandlers]
    public interface IWcfSoapService
    {
        /// <summary>
        /// This operation is mandatory, otherwise WCF will not expose an empty interface as a contract.
        /// </summary>
        [OperationContract]
        void DummyOperationForWCF();
    }
}
