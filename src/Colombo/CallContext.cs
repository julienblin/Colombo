using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    [Serializable]
    public class CallContext
    {
        public CallContext()
        {
            Properties = new Dictionary<string, string>();
        }

        public string TenantId { get; set; }

        public string UserId { get; set; }

        public string Culture { get; set; }

        public IDictionary<string, string> Properties { get; set; }
    }
}
