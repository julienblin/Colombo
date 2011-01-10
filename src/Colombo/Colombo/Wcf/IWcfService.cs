using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Colombo.Wcf
{
    [ServiceContract(Namespace = @"http://Colombo")]
    public interface IWcfService
    {
        [OperationContract(Name = @"Process", AsyncPattern=true)]
        [EmbedTypeInSerializer]
        IAsyncResult BeginProcessAsync(BaseRequest[] requests, AsyncCallback callback, object state);

        Response[] EndProcessAsync(IAsyncResult asyncResult);
    }
}
