using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(IColomboAlerter))]
    public abstract class ColomboAlerterContract : IColomboAlerter
    {
        public void Alert(IColomboAlert alert)
        {
            Contract.Requires<ArgumentNullException>(alert != null, "alert");
        }
    }
#pragma warning restore 1591
}
