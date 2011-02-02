#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Colombo.Wcf
{
    /// <summary>
    /// Implementation of the <see cref="IColomboService"/> service.
    /// </summary>
    [ServiceBehavior(
        IncludeExceptionDetailInFaults = true,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.PerCall
    )]
    public class ColomboService : IColomboService
    {
        /// <summary>
        /// Process requests asynchronously.
        /// </summary>
        public IAsyncResult BeginProcessAsync(BaseRequest[] requests, AsyncCallback callback, object state)
        {
            if (!WcfServices.DoNotManageMetaContextKeys && OperationContext.Current != null)
                foreach (var request in requests)
                {
                    request.Context[MetaContextKeys.EndpointAddressUri] =
                        OperationContext.Current.EndpointDispatcher.EndpointAddress.Uri.ToString();
                }

            var asyncResult = new ProcessAsyncResult(callback, state) { Requests = requests };

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

        /// <summary>
        /// Process requests asynchronously.
        /// </summary>
        public Response[] EndProcessAsync(IAsyncResult asyncResult)
        {
            using (var processResult = asyncResult as ProcessAsyncResult)
            {
                processResult.AsyncWaitHandle.WaitOne();

                if (processResult.Exception == null)
                    return processResult.Responses;

                // Preserve original stack trace.
                var remoteStackTraceString = typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);
                remoteStackTraceString.SetValue(processResult.Exception, processResult.Exception.StackTrace);
                throw processResult.Exception;
            }
        }
    }
}
