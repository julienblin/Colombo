using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Colombo.Wcf
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class WcfSoapService : IWcfSoapService
    {
        public void DummyOperationForWCF()
        {
        }
    }
}
