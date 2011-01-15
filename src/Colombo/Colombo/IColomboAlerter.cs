using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.ColomboAlerterContract))]
    public interface IColomboAlerter
    {
        void Alert(IColomboAlert alert);
    }
}
