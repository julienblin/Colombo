using Castle.Windsor;

namespace Colombo.Host
{
    public interface IWantToBeNotifiedWhenStartAndStop
    {
        void Start(IWindsorContainer container);

        void Stop(IWindsorContainer container);
    }
}
