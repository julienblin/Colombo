using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Configuration
{
    public class EmptyColomboConfiguration : IColomboConfiguration
    {
        public string GetTargetAddressFor(BaseRequest request, string targetType)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (targetType == null) throw new ArgumentNullException("targetType");
            Contract.EndContractBlock();

            return null;
        }
    }
}
