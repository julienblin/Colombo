using System;
using System.ServiceModel;

namespace Colombo.Wcf
{
    /// <summary>
    /// Service exposed by Colombo to allow remote communications.
    /// This service is dedicated to Colombo exchange communication, and not adapted for interoperability.
    /// See <see cref="IWcfSoapService"/> for exposing SOAP information correctly.
    /// </summary>
    [ServiceContract(Namespace = WcfServices.Namespace)]
    public interface IWcfColomboService
    {
        /// <summary>
        /// Process requests asynchronously.
        /// </summary>
        [OperationContract(Name = @"Process", AsyncPattern=true)]
        [EmbedTypeInSerializer]
        IAsyncResult BeginProcessAsync(BaseRequest[] requests, AsyncCallback callback, object state);

        /// <summary>
        /// Process requests asynchronously.
        /// </summary>
        Response[] EndProcessAsync(IAsyncResult asyncResult);
    }
}
