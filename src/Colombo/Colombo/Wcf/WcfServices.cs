using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel;
using System.Diagnostics.Contracts;

namespace Colombo.Wcf
{
    public static class WcfServices
    {
        public const string Namespace = @"http://Colombo";

        public static IKernel Kernel { get; set; }

        public static Response[] SyncProcess(BaseRequest[] requests)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            if (WcfServices.Kernel == null)
                throw new ColomboException("No Kernel has been registered. You must call WcfService.RegisterKernel before receiving any request.");

            ILocalRequestProcessor localRequestProcessor = null;
            try
            {
                localRequestProcessor = WcfServices.Kernel.Resolve<ILocalRequestProcessor>();
                Contract.Assume(localRequestProcessor != null);

                var undispatchableRequests = requests.Where(r => !localRequestProcessor.CanProcess(r));
                if (undispatchableRequests.Count() > 0)
                    throw new ColomboException(string.Format("Unable to dispatch requests {0} locally.", string.Join(", ", undispatchableRequests.Select(x => x.ToString()))));

                var responsesGroup = localRequestProcessor.Process(new List<BaseRequest>(requests));
                var responses = new Response[requests.Length];
                for (int i = 0; i < requests.Length; i++)
                {
                    var request = requests[i];
                    responses[i] = responsesGroup[request];
                }
                return responses;
            }
            catch (ComponentNotFoundException ex)
            {
                throw new ColomboException("No ILocalMessageProcessor could be resolved. You must register an ILocalMessageProcessor into the container.", ex);
            }
            finally
            {
                if (localRequestProcessor != null)
                    WcfServices.Kernel.ReleaseComponent(localRequestProcessor);
            }
        }
    }
}
