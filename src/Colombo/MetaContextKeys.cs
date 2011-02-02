using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public static class MetaContextKeys
    {
        public const string MetaPrefix = @"_";

        public const string SenderMachineName = MetaPrefix + @"senderMachineName";
        public const string HandlerMachineName = MetaPrefix + @"handlerMachineName";

        public const string EndpointAddressUri = MetaPrefix + @"endpointAddressUri";
    }
}
