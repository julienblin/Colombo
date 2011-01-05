﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    /// <summary>
    /// Interceptor for the <see cref="IRequestHandler.Handle.Send"/> operation.
    /// </summary>
    public interface IRequestHandlerHandleInterceptor : IColomboInterceptor
    {
        /// <summary>
        /// Called when the interceptor is asked to intercept.
        /// Must call <see cref="IColomboHandleInvocation.Proceed()"/> to allow the invocation chain to continue.
        /// </summary>
        /// <param name="nextInvocation">The next invocation to proceed.</param>
        void Intercept(IColomboHandleInvocation nextInvocation);
    }
}
