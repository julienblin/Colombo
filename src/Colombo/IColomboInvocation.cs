using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Colombo
{
    public interface IColomboInvocation
    {
        BaseRequest Request { get; }
        Response Response { get; set; }

        BaseRequest[] Requests { get; set; }
        Response[] Responses { get; set; }

        ColomboInvocationType InvocationType { get; }
        void Proceed();
    }

    public enum ColomboInvocationType
    {
        Send,
        ParallelSend
    }
}
