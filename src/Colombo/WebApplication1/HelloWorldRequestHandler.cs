using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Colombo;

namespace WebApplication1
{
    public class HelloWorldRequestHandler : SideEffectFreeRequestHandler<HelloWorldRequest, HelloWorldResponse>
    {
        protected override void Handle()
        {
            Response.Name = string.Format("Hello, {0}!", Request.Name);
        }
    }
}