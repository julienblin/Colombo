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
using System.Diagnostics.Contracts;
using System.Reflection;
using Castle.DynamicProxy;

namespace Colombo.Impl.Async
{
    internal class StatefulReponseInterceptor : IInterceptor
    {
        private static readonly MethodInfo ExceptionInternalPreserveStackTrace =
    typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly StatefulMessageBus statefulMessageBus;
        private readonly BaseSideEffectFreeRequest request;

        public StatefulReponseInterceptor(StatefulMessageBus statefulMessageBus, BaseSideEffectFreeRequest request)
        {
            if (statefulMessageBus == null) throw new ArgumentNullException("statefulMessageBus");
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            this.statefulMessageBus = statefulMessageBus;
            this.request = request;
        }

        Response internalResponse;

        public void Intercept(IInvocation invocation)
        {
            if (internalResponse == null)
            {
                internalResponse = statefulMessageBus.GetResponseForPendingRequest(request);
            }

            if (internalResponse != null)
            {
                try
                {
                    invocation.ReturnValue = invocation.Method.Invoke(internalResponse, invocation.Arguments);
                }
                catch (TargetInvocationException tie)
                {
                    // Propagate the inner exception so that the proxy throws the same exception as
                    // the real object would 
                    ExceptionInternalPreserveStackTrace.Invoke(tie.InnerException, new Object[] { });
                    throw tie.InnerException;
                }
            }
            else
            {
                invocation.Proceed();
            }
        }
    }
}
