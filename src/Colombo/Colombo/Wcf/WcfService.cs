using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Castle.MicroKernel;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Reflection;

namespace Colombo.Wcf
{
    [ServiceBehavior(
        IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.PerCall
    )]
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
                    Kernel.ReleaseComponent(localRequestProcessor);
            }
        }

        public IAsyncResult BeginProcessAsync(BaseRequest[] requests, AsyncCallback callback, object state)
        {
            var asyncResult = new ProcessAsyncResult(callback, state);
            asyncResult.Requests = requests;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    asyncResult.Responses = Process(asyncResult.Requests);
                }
                catch (Exception ex)
                {
                    asyncResult.Exception = ex;
                }
                finally
                {
                    asyncResult.OnCompleted();
                }
            });
            return asyncResult;
        }

        public Response[] EndProcessAsync(IAsyncResult asyncResult)
        {
            using (var processResult = asyncResult as ProcessAsyncResult)
            {
                processResult.AsyncWaitHandle.WaitOne();

                if (processResult.Exception == null)
                {
                    return processResult.Responses;
                }
                else
                {
                    // Preserve original stack trace.
                    FieldInfo remoteStackTraceString = typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);
                    remoteStackTraceString.SetValue(processResult.Exception, processResult.Exception.StackTrace);
                    throw processResult.Exception;
                }
            }
        }
    }
}
