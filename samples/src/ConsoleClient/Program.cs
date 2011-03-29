using System;
using System.Linq;
using Castle.Windsor;
using Colombo.Facilities;
using Colombo.Samples.Messages;

namespace Colombo.Samples.ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.ClientOnly());

            var messageBus = container.Resolve<IMessageBus>();

            Console.WriteLine("What's your name?");
            var name = Console.ReadLine().Trim();
            while (!string.IsNullOrEmpty(name))
            {
                var response = messageBus.Send(new HelloWorldRequest{ Name = name });

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
