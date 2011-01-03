using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Colombo.Configuration;
using Castle.Facilities.Logging;
using Colombo.Samples.Messages;

namespace Colombo.Samples.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new WindsorContainer();

            container.AddFacility<LoggingFacility>(f => f.LogUsing(LoggerImplementation.Console));
            container.AddFacility<ColomboFacility>();

            IMessageBus messageBus = container.Resolve<IMessageBus>();

            Console.WriteLine("What's your name?");
            var name = Console.ReadLine().Trim();
            while (!string.IsNullOrEmpty(name))
            {
                var request = new HelloWorldRequest { Name = name };
                var response = messageBus.Send(request);
                Console.WriteLine(response.Message);
                Console.WriteLine("What's your name?");
                name = Console.ReadLine().Trim();
            }
        }
    }
}
