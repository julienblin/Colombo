using Castle.Windsor;

namespace Colombo.Host
{
    public interface IWantToConfigureColombo
    {
        void ConfigureColombo(IWindsorContainer container);
    }
}
