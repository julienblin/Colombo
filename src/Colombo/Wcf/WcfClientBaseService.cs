using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Colombo.Wcf
{
    public class WcfClientBaseService : ClientBase<IWcfService>, IWcfService, IDisposable
    {
        public WcfClientBaseService() { }

		public WcfClientBaseService(string endpointConfigurationName) : base(endpointConfigurationName) { }

		public WcfClientBaseService(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

		public WcfClientBaseService(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

		public WcfClientBaseService(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress) { }

        public Response[] Process(BaseRequest[] requests)
        {
            return Channel.Process(requests);
        }

        public void Dispose()
        {
            if (State == CommunicationState.Faulted)
            {
                Abort();
            }
            else
            {
                try
                {
                    Close();
                }
                catch
                {
                    Abort();
                }
            }
        }
    }
}
