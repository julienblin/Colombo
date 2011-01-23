using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Samples.Messages;

namespace Colombo.Samples.Handlers
{
    public class HelloWorldRequestHandler : SideEffectFreeRequestHandler<HelloWorldRequest, HelloWorldResponse>
    {
        protected override void Handle()
        {
            Response.Message = string.Format("Hello, {0}!", Request.Name);
        }
    }
}
