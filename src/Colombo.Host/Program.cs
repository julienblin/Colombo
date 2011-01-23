using System;
using System.IO;
using Colombo.Host.Internal;
using Topshelf;
using Topshelf.Configuration.Dsl;

namespace Colombo.Host
{
    class Program
    {
        static int Main(string[] args)
        {
            var parser = new ArgsParser();
            parser.Parse(args);

            if (parser.Help)
            {
                DisplayHelpContent();
                return 0;
            }

            try
            {
                var baseDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                var assemblyScanner = new AssemblyScanner(baseDirectory);
                var configureThisEndpointType = assemblyScanner.FindUniqueConfigureThisEndPoint();

                var cfg = RunnerConfigurator.New(x =>
                {
                    x.ConfigureService<AppDomainBridge>(s =>
                    {
                        s.HowToBuildService(name => new AppDomainBridge(baseDirectory, configureThisEndpointType));
                        s.WhenStarted(h => h.Start());
                        s.WhenStopped(h => h.Stop());
                    });

                    if ((parser.Username != null) && (parser.Password != null))
                        x.RunAs(parser.Username, parser.Password);
                    else
                        x.RunAsNetworkService();

                    if (parser.StartManually)
                        x.DoNotStartAutomatically();

                    var endpointName = configureThisEndpointType.Assembly.GetName().Name;
                    var endpointId = string.Format("{0}_v{1}", endpointName, configureThisEndpointType.Assembly.GetName().Version);

                    x.SetDisplayName(parser.DisplayName ?? endpointId);
                    x.SetServiceName(parser.ServiceName ?? endpointId);
                    x.SetDescription(parser.Description ?? string.Format("Colombo Host Service for endpoint {0}.", endpointId));
                });

                Runner.Host(cfg, args);

                return 0;
            }
            catch (ColomboHostException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return -1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }
        
        private static void DisplayHelpContent()
        {
            try
            {
                var stream = typeof(Program).Assembly.GetManifestResourceStream("Colombo.Host.Content.Help.txt");
                if (stream != null)
                {
                    var helpText = new StreamReader(stream).ReadToEnd();
                    Console.WriteLine(helpText);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
