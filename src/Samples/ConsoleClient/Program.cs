using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using Colombo.Facilities;
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
            container.AddFacility<ColomboFacility>(f =>
            {
                f.ClientOnly();
            });

            IMessageBus messageBus = container.Resolve<IMessageBus>();

            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("fr-CA");

            Console.WriteLine("What's your name?");
            var name = Console.ReadLine().Trim();
            while (!string.IsNullOrEmpty(name))
            {
                var request = new HelloWorldRequest { Name = name };
                var response = messageBus.Send(request);
                if (response.IsValid())
                    Console.WriteLine(response.Message);
                else
                    Console.WriteLine(string.Join(", ", response.ValidationResults.Select(x => x.ErrorMessage)));
                Console.WriteLine("What's your name?");
                name = Console.ReadLine().Trim();
            }
        }
    }
}
