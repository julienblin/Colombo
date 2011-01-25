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
using System.Globalization;
using System.Threading;

namespace Colombo.Interceptors
{
    /// <summary>
    /// <see cref="IRequestHandlerHandleInterceptor"/> that uses a specific key in Context to set the CurrentThread.CurrentCulture and CurrentUICulture.
    /// </summary>
    public class CurrentCultureHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        /// <summary>
        /// Sets the current culture.
        /// </summary>
        public void Intercept(IColomboRequestHandleInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            if (nextInvocation.Request.Context.Keys.Contains(CurrentCultureConstant.CultureContextKey))
            {
                var contextValue = nextInvocation.Request.Context[CurrentCultureConstant.CultureContextKey];
                if (!string.IsNullOrWhiteSpace(contextValue))
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(contextValue);
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(contextValue);
                }
                else
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                }
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            }

            nextInvocation.Proceed();
        }

        /// <summary>
        /// Medium
        /// </summary>
        public int InterceptionPriority
        {
            get { return InterceptionPrority.Medium; }
        }
    }
}
