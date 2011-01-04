using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Impl
{
    public abstract class BaseColomboParallelInvocation : IColomboParallelInvocation
    {
        public BaseRequest[] Requests { get; set; }

        public Response[] Responses { get; set; }

        public abstract void Proceed();
    }
}
