using Castle.Windsor;

namespace Colombo.Host
{
    public interface IWantToRegisterMessageHandlers
    {
        void RegisterMessageHandlers(IWindsorContainer container);
    }
}
