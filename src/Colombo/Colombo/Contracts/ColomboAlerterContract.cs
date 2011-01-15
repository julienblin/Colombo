using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IColomboAlerter))]
    public abstract class ColomboAlerterContract : IColomboAlerter
    {
        public void Alert(IColomboAlert alert)
        {
            Contract.Requires<ArgumentNullException>(alert != null, "alert");
        }
    }
}
