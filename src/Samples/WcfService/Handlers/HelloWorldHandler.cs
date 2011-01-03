using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Colombo.Samples.Messages;

namespace Colombo.Samples.WcfService.Handlers
{
    public class HelloWorldHandler : RequestHandler<HelloWorldRequest, HelloWorldResponse>
    {
        public override void Handle()
        {
            Response.Message = string.Format("Hello, {0}", Request.Name);
        }
    }
}