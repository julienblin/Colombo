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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Castle.DynamicProxy;
using Colombo.Impl.Async;
using Colombo.Interceptors;

namespace Colombo.TestSupport
{
    /// <summary>
    /// Base static classes for unit test support
    /// </summary>
    public static class ColomboTest
    {
        /// <summary>
        /// All assertions
        /// </summary>
        public static class AssertThat
        {
            /// <summary>
            /// Will raise a <see cref="ColomboTestSupportException"/> if any message (i.e. requests and responses)
            /// in the assembly that contains <typeparamref name="T"/> cannot be used by Colombo.
            /// </summary>
            public static void AllMessagesAreConformInAssemblyThatContains<T>()
            {
                AllMessagesAreConformInAssembly(typeof(T).Assembly);
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestSupportException"/> if any message (i.e. requests and responses)
            /// in the assembly <paramref name="assembly"/> cannot be used by Colombo.
            /// </summary>
            public static void AllMessagesAreConformInAssembly(Assembly assembly)
            {
                AllRequestsAreConformInAssembly(assembly);
                AllResponsesAreConformInAssembly(assembly);
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestSupportException"/> if any request
            /// in the assembly that contains <typeparamref name="T"/> cannot be used by Colombo.
            /// </summary>
            public static void AllRequestsAreConformInAssemblyThatContains<T>()
            {
                AllRequestsAreConformInAssembly(typeof (T).Assembly);
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestSupportException"/> if any request
            /// in the assembly <paramref name="assembly"/> cannot be used by Colombo.
            /// </summary>
            public static void AllRequestsAreConformInAssembly(Assembly assembly)
            {
                var allRequestsTypes =
                    assembly.GetTypes().Where(
                        x => typeof (BaseRequest).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface);
                foreach (var requestType in allRequestsTypes)
                {
                    RequestIsConform(requestType);
                }
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestSupportException"/> if the <typeparam name="TRequest" /> cannot be used by Colombo.
            /// </summary>
            public static void RequestIsConform<TRequest>()
                where TRequest: BaseRequest
            {
                RequestIsConform(typeof(TRequest));
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestSupportException"/> if the <paramref name="requestType"/> cannot be used by Colombo.
            /// </summary>
            public static void RequestIsConform(Type requestType)
            {
                BaseRequest request = null;
                try
                {
                    request = (BaseRequest)Activator.CreateInstance(requestType);
                }
                catch (Exception ex)
                {
                    throw new ColomboTestSupportException(string.Format("Request {0} cannot be instantiated. Probably because you forgot to include a default constructor.", requestType), ex);
                }

                using (var stream = new MemoryStream())
                {
                    try
                    {
                        var serializer = new DataContractSerializer(requestType);
                        serializer.WriteObject(stream, request);
                        stream.Position = 0;
                        serializer.ReadObject(stream);
                    }
                    catch (Exception ex)
                    {
                        throw new ColomboTestSupportException(string.Format("Request {0} should be serializable using the DataContractSerializer.", requestType), ex);
                    }
                }

                var enableCacheAttribute = request.GetCustomAttribute<EnableCacheAttribute>();
                if(enableCacheAttribute != null)
                {
                    if(!request.IsSideEffectFree)
                        throw new ColomboTestSupportException(string.Format("Request {0} has EnableCache attribute but is not side effect free.", requestType));

                    try
                    {
                        request.GetCacheKey();
                    }
                    catch (ColomboException ex)
                    {
                        throw new ColomboTestSupportException(string.Format("Request {0} has EnableCache attribute but do not implement GetCacheKey().", requestType), ex);
                    }
                    catch(Exception)
                    {
                        // Can be normal.
                    }
                }
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestSupportException"/> if any response
            /// in the assembly that contains <typeparamref name="T"/> cannot be used by Colombo.
            /// </summary>
            public static void AllResponsesAreConformInAssemblyThatContains<T>()
            {
                AllResponsesAreConformInAssembly(typeof(T).Assembly);
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestSupportException"/> if any response
            /// in the assembly <paramref name="assembly"/> cannot be used by Colombo.
            /// </summary>
            public static void AllResponsesAreConformInAssembly(Assembly assembly)
            {
                var allResponsesTypes =
                    assembly.GetTypes().Where(
                        x => typeof(Response).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface);
                foreach (var responseType in allResponsesTypes)
                {
                    ResponseIsConform(responseType);
                }
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestSupportException"/> if the <typeparam name="TResponse" /> cannot be used by Colombo.
            /// </summary>
            public static void ResponseIsConform<TResponse>()
                where TResponse : Response
            {
                ResponseIsConform(typeof(TResponse));
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestSupportException"/> if the <paramref name="responseType"/> cannot be used by Colombo.
            /// </summary>
            public static void ResponseIsConform(Type responseType)
            {
                try
                {
                    Activator.CreateInstance(responseType);
                }
                catch (Exception ex)
                {
                    throw new ColomboTestSupportException(string.Format("Response {0} cannot be instantiated. Probably because you forgot to include a default constructor.", responseType), ex);
                }

                using (var stream = new MemoryStream())
                {
                    try
                    {
                        var serializer = new DataContractSerializer(responseType);
                        var request = Activator.CreateInstance(responseType);
                        serializer.WriteObject(stream, request);
                        stream.Position = 0;
                        serializer.ReadObject(stream);
                    }
                    catch (Exception ex)
                    {
                        throw new ColomboTestSupportException(string.Format("Response {0} should be serializable using the DataContractSerializer.", responseType), ex);
                    }
                }

                try
                {
                    var options = new ProxyGenerationOptions(new NonVirtualCheckProxyGenerationHook());
                    var proxyGen = new ProxyGenerator();
                    proxyGen.CreateClassProxy(responseType, options);
                }
                catch (Exception ex)
                {
                    throw new ColomboTestSupportException(string.Format("Response {0} cannot be proxied, probably because one or several of its members are not virtual.", responseType), ex);
                }
            }
        }
    }
}
