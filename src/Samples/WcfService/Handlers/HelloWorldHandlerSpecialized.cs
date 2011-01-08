using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Colombo.Samples.Messages;

namespace Colombo.Samples.WcfService.Handlers
{
    [ChooseWhenRequestContextContains("Specialized")]
    public class HelloWorldHandlerSpecialized : SideEffectFreeRequestHandler<HelloWorldRequest, HelloWorldResponse>
    {
        public override void Handle()
        {
            Response.Message = string.Format("This is specialized, {0}!", Request.Name);
        }
    }
}