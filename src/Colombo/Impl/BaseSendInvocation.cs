using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Impl
{
    public abstract class BaseSendInvocation : IColomboSendInvocation
    {
        public IList<BaseRequest> Requests { get; set; }
        public ResponsesGroup Responses { get; set; }

        public abstract void Proceed();
    }
}
