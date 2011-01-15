﻿using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;

namespace Colombo.Interceptors
{
    /// <summary>
    /// <see cref="IRequestHandlerHandleInterceptor"/> that places in a specific key in Context the CurrentThread.CurrentUICulture .
    /// </summary>
    public class CurrentCultureSendInterceptor : IMessageBusSendInterceptor
    {
        public void Intercept(IColomboSendInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            foreach (var request in nextInvocation.Requests)
            {
                if ((Thread.CurrentThread.CurrentUICulture != CultureInfo.InvariantCulture))
                    request.Context[CurrentCultureConstant.CultureContextKey] = Thread.CurrentThread.CurrentUICulture.Name;
            }

            nextInvocation.Proceed();
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.Medium; }
        }
    }
}
