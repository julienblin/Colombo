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
        [OperationContract(Name = @"Send")]
        [EmbedTypeInSerializer]
        Response Send(BaseRequest request);

        [OperationContract(Name = @"ParallelSend")]
        [EmbedTypeInSerializer]
        Response[] ParallelSend(BaseRequest[] requests);
    }
}
