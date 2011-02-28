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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Colombo.Host.Internal
{
    internal class AssemblyScanner
    {
        private const string EndPointAppSettingsKey = @"EndpointType";

        private readonly DirectoryInfo baseDirectory;

        internal AssemblyScanner(DirectoryInfo baseDirectory)
        {
            this.baseDirectory = baseDirectory;
        }

        internal Type GetEndPointType()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings[EndPointAppSettingsKey]))
                return FindUniqueConfigureThisEndPoint();

            var endPointTypeStr = ConfigurationManager.AppSettings[EndPointAppSettingsKey];
            var endPointType = Type.GetType(endPointTypeStr);

            if(endPointType == null)
                throw new ColomboHostException(string.Format("Unable to load type {0} specified by app settings key {1}.", endPointTypeStr, EndPointAppSettingsKey));

            if (!typeof(IAmAnEndpoint).IsAssignableFrom(endPointType))
                throw new ColomboHostException(string.Format("Type {0} specified by app settings key {1} doesn't implement {2}: Unable to start the host.",
                    endPointType, EndPointAppSettingsKey, typeof(IAmAnEndpoint)));

            return endPointType;
        }

        internal Type FindUniqueConfigureThisEndPoint()
        {
            var scannableAssemblies = GetScannableAssemblies();
            var configureThisEndpointTypes = GetConfigureThisEndPointTypes(scannableAssemblies).ToArray();

            if (configureThisEndpointTypes.Length == 0)
            {
                throw new ColomboHostException(string.Format("No endpoint configuration found in scanned assemblies.\n" +
                    "This usually happens when Colombo.Host fails to load your assembly containing {0}.\n" +
                    "Scanned path: {1}.\n" +
                    "Assemblies found: {2}.\n" +
                    "You can specify a type using the app settings key {3} in {4}:\n" +
                    "<appSettings>\n<add key=\"{3}\" value=\"[AssemblyQualifiedName]\"/>\n</appSettings>",
                    typeof(IAmAnEndpoint),
                    AppDomain.CurrentDomain.BaseDirectory,
                    string.Join(", ", scannableAssemblies.Select(a => a.GetName().Name)),
                    EndPointAppSettingsKey,
                    Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile)));
            }

            if (configureThisEndpointTypes.Length > 1)
            {
                throw new ColomboHostException(string.Format("Host doesn't support hosting of multiple endpoints.\n" +
                    "Endpoint classes found: {0}.\n" +
                    "You may have some old assemblies in your runtime directory." +
                    " Try right-clicking your VS project, and selecting 'Clean'.\n" +
                    "You can specify a type using the app settings key {1} in {2}:\n" +
                    "<appSettings>\n<add key=\"{1}\" value=\"[AssemblyQualifiedName]\"/>\n</appSettings>",
                    string.Join(", ", configureThisEndpointTypes.Select( e => e.AssemblyQualifiedName).ToArray()),
                    EndPointAppSettingsKey,
                    Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile)));
            }

            return configureThisEndpointTypes[0];
        }

        internal static IEnumerable<Type> GetConfigureThisEndPointTypes(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(assembly => assembly.GetTypes().Where(t => typeof(IAmAnEndpoint).IsAssignableFrom(t)
                                                                                    && !t.IsInterface
                                                                                    && !t.IsAbstract
                                                                                    && t.IsPublic
                                                         ));
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
                    //will throw if assembly can't be loaded
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
