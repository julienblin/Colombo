using Castle.Windsor;

namespace Colombo.Host
{
    public interface IWantToConfigureLogging
    {
        void ConfigureLogging(IWindsorContainer container);
    }
}
