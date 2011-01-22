using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Colombo;

namespace WebApplication1
{
    public class HelloWorldRequest : SideEffectFreeRequest<HelloWorldResponse>
    {
        public string Name { get; set; }
    }
}