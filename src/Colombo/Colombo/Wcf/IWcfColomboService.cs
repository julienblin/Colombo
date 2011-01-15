using System;
using System.ServiceModel;

namespace Colombo.Wcf
{
    [ServiceContract(Namespace = WcfServices.Namespace)]
    public interface IWcfColomboService
    {
        [OperationContract(Name = @"Process", AsyncPattern=true)]
        [EmbedTypeInSerializer]
        IAsyncResult BeginProcessAsync(BaseRequest[] requests, AsyncCallback callback, object state);

        Response[] EndProcessAsync(IAsyncResult asyncResult);
    }
}
