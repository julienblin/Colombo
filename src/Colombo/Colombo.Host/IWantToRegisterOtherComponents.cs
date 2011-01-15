using Castle.Windsor;

namespace Colombo.Host
{
    public interface IWantToRegisterOtherComponents
    {
        void RegisterOtherComponents(IWindsorContainer container);
    }
}
