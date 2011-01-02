using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.ColomboConfigurationContract))]
    public interface IColomboConfiguration
    {
        string GetTargetAddressFor(BaseRequest request, string targetType);
    }
}
