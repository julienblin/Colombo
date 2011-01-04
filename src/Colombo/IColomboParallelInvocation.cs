using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public interface IColomboParallelInvocation
    {
        BaseRequest[] Requests { get; set; }
        Response[] Responses { get; set; }
        void Proceed();
    }
}
