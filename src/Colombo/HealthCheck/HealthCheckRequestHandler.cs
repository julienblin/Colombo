using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.HealthCheck
{
    public class HealthCheckRequestHandler : SideEffectFreeRequestHandler<HealthCheckRequest, ACKResponse>
    {
        public override void Handle()
        {
            
        }
    }
}
