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
        /// Static <see cref="IKernel"/> reference. - That means that when using WCF Services, only one container is allowed per AppDomain.
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

        /// <summary>
        /// Process a request by using the Send method of the currently registeres <see cref="IMessageBus"/>.
        /// </summary>
        public static Response Process(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            if (Kernel == null)
                throw new ColomboException("No Kernel has been registered. You must asign a Castle.IKernel to WcfServices.Kernel before receiving any request.");

            IMessageBus messageBus = null;
            try
            {
                messageBus = Kernel.Resolve<IMessageBus>();
                return messageBus.Send(request);
            }
            catch (ComponentNotFoundException ex)
            {
                throw new ColomboException("No IMessageBus could be resolved. You must register an IMessageBus into the container.", ex);
            }
            finally
            {
                if (messageBus != null)
                    Kernel.ReleaseComponent(messageBus);
            }
        }
    }
}
