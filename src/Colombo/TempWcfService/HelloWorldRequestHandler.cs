using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Colombo;

namespace TempWcfService
{
    public class HelloWorldRequestHandler : RequestHandler<HelloWorldRequest, HelloWorldResponse>
    {
        public override void Handle()
        {
            Response.Name = Request.Name;
        }
    }
}