using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Configuration;

namespace Colombo.Wcf
{
    public class WcfClientMessageProcessor : IMessageProcessor
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        public bool CanSend(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            if (WcfConfigClientSection == null)
                return false;

            var requestGroupName = request.GetGroupName();

            foreach (ChannelEndpointElement endPoint in WcfConfigClientSection.Endpoints)
            {
                if (endPoint.Name.Equals(requestGroupName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public Response Send(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            Response response = null;
            using (var clientBase = CreateClientBase(request))
            {
                Logger.DebugFormat("Sending {0} to {1}...", request, clientBase.Endpoint.Address.Uri);
                response = clientBase.Send(request);
            }

            if (response == null)
                throw new ColomboException("Internal error : response should not be null");

            return response;
        }

        public Response[] ParallelSend(BaseRequest[] requests)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            throw new NotImplementedException();
        }

        private ClientSection clientSection = null;
        private bool clientSectionHasBeenLoaded = false;

        public ClientSection WcfConfigClientSection
        {
            get {
                if (!clientSectionHasBeenLoaded)
                {
                    Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    ServiceModelSectionGroup serviceModelGroup = ServiceModelSectionGroup.GetSectionGroup(configuration);
                    if (serviceModelGroup != null)
                        clientSection = serviceModelGroup.Client;

                    clientSectionHasBeenLoaded = true;
                }
                return clientSection;
            }
        }

        public WcfClientBaseService CreateClientBase(BaseRequest request)
        {
            Contract.Assume(request != null);
            try
            {
                return new WcfClientBaseService(request.GetGroupName());
            }
            catch (InvalidOperationException ex)
            {
                var errorMessage = string.Format("Unable to create a ClientBase for {0}: Did you create a wcf client endPoint with the name {1}?", request, request.GetGroupName());
                Logger.Error(errorMessage, ex);
                throw new ColomboException(errorMessage, ex);
            }
        }
    }
}
