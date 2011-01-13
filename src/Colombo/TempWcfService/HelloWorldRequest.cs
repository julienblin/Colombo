using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Colombo;

namespace TempWcfService
{
    public class HelloWorldRequest : Request<HelloWorldResponse>
    {
        public string Name { get; set; }
    }
}