using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Impl
{
    /// <summary>
    /// Base class for <see cref="IColomboSendInvocation"></see>.
    /// </summary>
    public abstract class BaseSendInvocation : IColomboSendInvocation
    {
        public IList<BaseRequest> Requests { get; set; }
        public ResponsesGroup Responses { get; set; }

        public abstract void Proceed();
    }
}
