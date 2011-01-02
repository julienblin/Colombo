using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IColomboConfiguration))]
    public abstract class ColomboConfigurationContract : IColomboConfiguration
    {
        string IColomboConfiguration.GetTargetAddressFor(BaseRequest request, string targetType)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Requires<ArgumentNullException>(targetType != null, "targetType");
            return default(string);
        }
    }
}
