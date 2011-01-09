using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Impl
{
    /// <summary>
    /// Base class for <see cref="IColomboHandleInvocation"></see>.
    /// </summary>
    public abstract class BaseHandleInvocation : IColomboHandleInvocation
    {
        public BaseRequest Request { get; set; }

        public Response Response { get; set; }

        public abstract void Proceed();
    }
}
