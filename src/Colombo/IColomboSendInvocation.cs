using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public interface IColomboSendInvocation
    {
        IList<BaseRequest> Requests { get; set; }
        ResponsesGroup Responses { get; set; }
        void Proceed();
    }
}
