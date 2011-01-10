using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Colombo.Host.Internal
{
    internal class AssemblyScanner
    {
        private readonly DirectoryInfo baseDirectory;

        internal AssemblyScanner(DirectoryInfo baseDirectory)
        {
            this.baseDirectory = baseDirectory;
        }

        internal Type FindUniqueConfigureThisEndPoint()
        {
            var scannableAssemblies = GetScannableAssemblies();
            var configureThisEndpointTypes = GetConfigureThisEndPointTypes(scannableAssemblies).ToArray();

            if (configureThisEndpointTypes.Length == 0)
            {
                throw new ColomboHostException(string.Format("No endpoint configuration found in scanned assemblies.\n" +
                    "This usually happens when Colombo.Host fails to load your assembly contaning IConfigureThisEndpoint.\n" +
                    "Scanned path: {0}.\n" +
                    "Assemblies found: {1}.",
                    AppDomain.CurrentDomain.BaseDirectory,
                    string.Join(", ", scannableAssemblies.Select(a => a.GetName().Name))));
            }

            if (configureThisEndpointTypes.Length > 1)
            {
                throw new ColomboHostException(string.Format("Host doesn't support hosting of multiple endpoints.\n" +
                                                             "Endpoint classes found: {0}.\n" +
                                                             "You may have some old assemblies in your runtime directory." +
                                                             " Try right-clicking your VS project, and selecting 'Clean'.",
                                                             string.Join(", ", configureThisEndpointTypes.Select( e => e.AssemblyQualifiedName).ToArray())));
            }

            return configureThisEndpointTypes[0];
        }

        internal IEnumerable<Type> GetConfigureThisEndPointTypes(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => typeof(IAmAnEndpoint).IsAssignableFrom(t)
                                                                    && !t.IsInterface
                                                                    && !t.IsAbstract
                                                              )
                        )
                {
                    yield return type;
                }
            }
        }

        internal IEnumerable<Assembly> GetScannableAssemblies()
        {
            var assemblyFiles = baseDirectory.GetFiles("*.dll", SearchOption.AllDirectories)
                                .Union(baseDirectory.GetFiles("*.exe", SearchOption.AllDirectories));

            foreach (var assemblyFile in assemblyFiles)
            {
                Assembly assembly;

                try
                {
                    assembly = Assembly.LoadFrom(assemblyFile.FullName);
                    //will throw if assembly cant be loaded
                    assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                yield return assembly;
            }
        }
    }
}
