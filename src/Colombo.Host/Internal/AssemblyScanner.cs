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
using System.IO;
using System.Linq;
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
