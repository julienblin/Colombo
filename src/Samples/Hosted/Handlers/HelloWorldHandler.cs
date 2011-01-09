using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo;
using Colombo.Samples.Messages;

namespace Hosted.Handlers
{
    public class HelloWorldHandler : SideEffectFreeRequestHandler<HelloWorldRequest, HelloWorldResponse>
    {
        public override void Handle()
        {
            Response.Message = string.Format("Hello {0} from hosted service!", Request.Name);
        }
    }
}
