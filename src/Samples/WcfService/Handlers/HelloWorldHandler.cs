﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Colombo.Samples.Messages;
using System.Threading;
using System.Resources;
using Colombo.Samples.WcfService.L10n;

namespace Colombo.Samples.WcfService.Handlers
{
    public class HelloWorldHandler : SideEffectFreeRequestHandler<HelloWorldRequest, HelloWorldResponse>
    {
        protected override void Handle()
        {
            Response.Message = string.Format(Resources.Hello, Request.Name);
        }
    }
}