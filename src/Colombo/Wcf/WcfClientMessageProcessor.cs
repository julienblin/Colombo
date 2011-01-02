using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace Colombo.Wcf
{
    public class WcfClientMessageProcessor : IMessageProcessor
    {
        public const string WcfEndPointType = @"wcf";

        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly IColomboConfiguration colomboConfiguration;

        public WcfClientMessageProcessor(IColomboConfiguration colomboConfiguration)
        {
            if (colomboConfiguration == null) throw new ArgumentNullException("colomboConfiguration");
            Contract.EndContractBlock();

            this.colomboConfiguration = colomboConfiguration;
        }

        public bool CanSend<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            return (colomboConfiguration.GetTargetAddressFor(request, WcfEndPointType) != null);
        }

        public TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            using (var clientBase = CreateClientBase(request))
            {
                return (TResponse)clientBase.Send(request);
            }
        }

        public WcfClientBaseService CreateClientBase(BaseRequest request)
        {
            Contract.Assume(request != null);

            var configAddress = colomboConfiguration.GetTargetAddressFor(request, WcfEndPointType);
            Uri uri = null;
            if (!Uri.TryCreate(configAddress, UriKind.Absolute, out uri))
                LogAndThrowError("Malformed Uri for the endpoint address associated with request {0}: {1}", request, configAddress);

            Binding binding = null;
            switch (uri.Scheme)
            {
                case "http":
                    binding = new BasicHttpBinding();
                    break;
                case "net.tcp":
                    binding = new NetTcpBinding();
                    break;
                default:
                    LogAndThrowError("Unrecognized Uri scheme for the endpoint address associated with request {0}: {1}. Valid values are: http, net.tcp.",
                        request,
                        uri.Scheme);
                    break;
            }

            var endPointAddress = new EndpointAddress(uri);

            return new WcfClientBaseService(binding, endPointAddress);
        }

        private void LogAndThrowError(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");
            Contract.EndContractBlock();

            var errorMessage = string.Format(format, args);
            Logger.Error(errorMessage);
            throw new ColomboException(errorMessage);
        }
    }
}
