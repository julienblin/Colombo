using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;

namespace Colombo.Wcf
{
    public class WcfService : IWcfService
    {
        private static IKernel Kernel { get; set; }

        public static void RegisterKernel(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            Contract.EndContractBlock();

            Kernel = kernel;
        }

        public Response Send(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            if (Kernel == null)
                throw new ColomboException("No Kernel has been registered. You must call WcfService.RegisterKernel before receiving any request.");

            try
            {
                var localMessageProcessor = Kernel.Resolve<ILocalMessageProcessor>();
                if (!localMessageProcessor.CanSend(request))
                    throw new ColomboException(string.Format("Unable to dispatch request {0} locally.", request));

                return localMessageProcessor.Send(request);
            }
            catch (ComponentNotFoundException ex)
            {
                throw new ColomboException("No ILocalMessageProcessor could be resolved. You must register an ILocalMessageProcessor into the container.", ex);
            }
        }
    }
}
