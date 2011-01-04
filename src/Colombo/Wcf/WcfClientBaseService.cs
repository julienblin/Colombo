using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Colombo.Wcf
{
    public class WcfClientBaseService : ClientBase<IWcfService>, IWcfService
    {
        public WcfClientBaseService(string endpointConfigurationName) : base(endpointConfigurationName) { }

        public WcfClientBaseService(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }

        public Response Send(BaseRequest request)
        {
            return Channel.Send(request);
        }

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch (Exception)
            {
                Abort();
            }
        }
    }
}
