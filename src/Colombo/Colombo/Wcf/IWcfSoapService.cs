using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Colombo.Wcf
{
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
