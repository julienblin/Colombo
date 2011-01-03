using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IColomboAlerter))]
    public abstract class ColomboAlerterContract : IColomboAlerter
    {
        void IColomboAlerter.Alert(IColomboAlert alert)
        {
            Contract.Requires<ArgumentNullException>(alert != null, "alert");
        }
    }
}
