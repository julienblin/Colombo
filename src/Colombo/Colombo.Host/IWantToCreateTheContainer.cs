using Castle.Windsor;

namespace Colombo.Host
{
    public interface IWantToCreateTheContainer
    {
        IWindsorContainer CreateContainer();
    }
}
