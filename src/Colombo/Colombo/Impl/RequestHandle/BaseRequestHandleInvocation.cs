using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Impl.RequestHandle
{
    /// <summary>
    /// Base class for <see cref="IColomboRequestHandleInvocation"></see>.
    /// </summary>
    public abstract class BaseRequestHandleInvocation : IColomboRequestHandleInvocation
    {
        public BaseRequest Request { get; set; }

        public Response Response { get; set; }

        public abstract void Proceed();
    }
}
