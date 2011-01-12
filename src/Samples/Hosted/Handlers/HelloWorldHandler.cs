using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo;
using Colombo.Samples.Messages;

namespace Colombo.Samples.Hosted.Handlers
{
    public class HelloWorldHandler : SideEffectFreeRequestHandler<HelloWorldRequest, HelloWorldResponse>
    {
        public IMessageBus MessageBus { get; set; }

        public override void Handle()
        {
            Response.Message = string.Format("Hello {0} from hosted service!", Request.Name);

            var notification = CreateNotification<HelloWorldNotification>();
            notification.Name = Request.Name;
            MessageBus.Notify(notification);
        }
    }
}
