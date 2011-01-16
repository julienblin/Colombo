using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.MicroKernel;

namespace Colombo.Wcf
{
    /// <summary>
    /// Static class that can be used by all WCF services.
    /// </summary>
    public static class WcfServices
    {
        /// <summary>
        /// The namespace to use with services.
        /// </summary>
        public const string Namespace = @"http://Colombo";

        /// <summary>
        /// Static <see cref="IKernel"/> reference. - That means when using WCF Services, only one container is allowed per AppDomain.
        /// </summary>
        public static IKernel Kernel { get; set; }

        /// <summary>
        /// Processes the requests locally using <see cref="ILocalRequestProcessor"/>.
        /// </summary>
        public static Response[] ProcessLocally(BaseRequest[] requests)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            if (Kernel == null)
                throw new ColomboException("No Kernel has been registered. You must asign a Castle.IKernel to WcfServices.Kernel before receiving any request.");

            ILocalRequestProcessor localRequestProcessor = null;
            try
            {
                localRequestProcessor = Kernel.Resolve<ILocalRequestProcessor>();
                Contract.Assume(localRequestProcessor != null);

                var undispatchableRequests = requests.Where(r => !localRequestProcessor.CanProcess(r));
                if (undispatchableRequests.Count() > 0)
                    throw new ColomboException(string.Format("Unable to dispatch requests {0} locally.", string.Join(", ", undispatchableRequests.Select(x => x.ToString()))));

                var responsesGroup = localRequestProcessor.Process(new List<BaseRequest>(requests));
                var responses = new Response[requests.Length];
                for (var i = 0; i < requests.Length; i++)
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
                    Kernel.ReleaseComponent(localRequestProcessor);
            }
        }
    }
}
