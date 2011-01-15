﻿using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(IMessageBusNotifyInterceptor))]
    public abstract class MessageBusNotifyInterceptorContract : IMessageBusNotifyInterceptor
    {
        public void Intercept(IColomboNotifyInvocation invocation)
        {
            Contract.Requires<ArgumentNullException>(invocation != null, "invocation");
            throw new NotImplementedException();
        }

        public int InterceptionPriority
        {
            get { throw new NotImplementedException(); }
        }
    }
#pragma warning restore 1591
}
