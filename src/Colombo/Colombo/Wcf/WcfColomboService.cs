using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Reflection;
using Castle.MicroKernel;

namespace Colombo.Wcf
{
    [ServiceBehavior(
        IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.PerCall
    )]
    public class WcfColomboService : IWcfColomboService
    {
        public IAsyncResult BeginProcessAsync(BaseRequest[] requests, AsyncCallback callback, object state)
        {
            var asyncResult = new ProcessAsyncResult(callback, state);
            asyncResult.Requests = requests;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    asyncResult.Responses = WcfServices.ProcessLocally(asyncResult.Requests);
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
