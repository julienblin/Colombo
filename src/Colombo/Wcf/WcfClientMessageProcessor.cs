using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;

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
            return (colomboConfiguration.GetTargetAddressFor(request, WcfEndPointType) != null);
        }

        public TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }
    }
}
