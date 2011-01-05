using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Castle.MicroKernel;
using System.Diagnostics.Contracts;

namespace Colombo.Wcf
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class WcfService : IWcfService
    {
        private static IKernel Kernel { get; set; }

        public static void RegisterKernel(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            Contract.EndContractBlock();

            Kernel = kernel;
        }

        public Response[] Process(BaseRequest[] requests)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            if (Kernel == null)
                throw new ColomboException("No Kernel has been registered. You must call WcfService.RegisterKernel before receiving any request.");

            try
            {
                var localRequestProcessor = Kernel.Resolve<ILocalRequestProcessor>();
                var undispatchableRequests = requests.Where(r => !localRequestProcessor.CanProcess(r));
                if (undispatchableRequests.Count() > 0)
                    throw new ColomboException(string.Format("Unable to dispatch requests {0} locally.", string.Join(", ", undispatchableRequests.Select(x => x.ToString()))));

                var responsesGroup = localRequestProcessor.Process(new List<BaseRequest>(requests));
                var responses = new Response[requests.Length];
                for (int i = 0; i < requests.Length; i++)
                {
                    responses[i] = responsesGroup[requests[i]];
                }
                return responses;
            }
            catch (ComponentNotFoundException ex)
            {
                throw new ColomboException("No ILocalMessageProcessor could be resolved. You must register an ILocalMessageProcessor into the container.", ex);
            }
        }
    }
}
