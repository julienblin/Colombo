using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;

namespace Colombo.Host
{
    public interface IWantToRegisterMessageHandlers
    {
        void RegisterMessageHandlers(IWindsorContainer container);
    }
}
