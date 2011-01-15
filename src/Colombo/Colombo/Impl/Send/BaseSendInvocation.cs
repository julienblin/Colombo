using System.Collections.Generic;

namespace Colombo.Impl.Send
{
    /// <summary>
    /// Base class for <see cref="IColomboSendInvocation"></see>.
    /// </summary>
    internal abstract class BaseSendInvocation : IColomboSendInvocation
    {
        public IList<BaseRequest> Requests { get; set; }
        public ResponsesGroup Responses { get; set; }

        public abstract void Proceed();
    }
}
