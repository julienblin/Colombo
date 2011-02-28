#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.IO;
using Colombo.Host.Internal;
using Topshelf;
using Topshelf.Configuration.Dsl;

namespace Colombo.Host
{
    class Program
    {
        public static int Main(string[] args)
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
                var configureThisEndpointType = assemblyScanner.GetEndPointType();

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
