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
using System.Linq;
using Castle.Core.Logging;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Interceptor that validates the request annotated with <see cref="RequiredInContextAttribute"/>.
    /// Throws a <see cref="RequiredInContextException"/> if the key is null or empty.
    /// </summary>
    public class RequiredInContextHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        /// <summary>
        /// Performs the validation.
        /// </summary>
        public void Intercept(IColomboRequestHandleInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            var reqAttributes = nextInvocation.Request.GetCustomAttributes<RequiredInContextAttribute>(true);
            if (reqAttributes.Length > 0)
            {
                var keys = reqAttributes.SelectMany(x => x.GetKeys()).Distinct();
                var missingKeys = keys.Where(key => !nextInvocation.Request.Context.Keys.Contains(key) || string.IsNullOrEmpty(nextInvocation.Request.Context[key])).ToList();

                if (missingKeys.Count > 0)
                    LogAndThrowError("Missing keys in Context: {0}", string.Join(", ", missingKeys));
            }

            nextInvocation.Proceed();
        }

        /// <summary>
        /// Medium.
        /// </summary>
        public int InterceptionPriority
        {
            get { return InterceptionPrority.Medium; }
        }

        private void LogAndThrowError(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");
            Contract.EndContractBlock();

            var errorMessage = string.Format(format, args);
            Logger.Error(errorMessage);
            throw new RequiredInContextException(errorMessage);
        }
    }
}
