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
using System.Threading.Tasks;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle;

namespace Colombo.Facilities
{
    /// <summary>
    /// Inside a request handling activity, resolve one instance per handling.
    /// Outside a request handling, resolve one instance per thread.
    /// </summary>
    public class PerRequestHandlingLifestyleManager : AbstractLifestyleManager
    {
        [ThreadStatic]
        private static Dictionary<IComponentActivator, IDictionary<int?, object>> map;

        private static Dictionary<IComponentActivator, IDictionary<int?, object>> Map
        {
            get { return map ?? (map = new Dictionary<IComponentActivator, IDictionary<int?, object>>()); }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            foreach (var instance in Map.Values)
            {
                base.Release(instance);
            }
        }

        /// <summary>
        /// Return the component instance based on the lifestyle semantic.
        /// </summary>
        public override object Resolve(CreationContext context)
        {
            IDictionary<int?, object> instanceTaskMap;

            var dictionary = Map;
            if (!dictionary.TryGetValue(ComponentActivator, out instanceTaskMap))
            {
                instanceTaskMap = new Dictionary<int?, object>();
                instanceTaskMap[Task.CurrentId ?? Int32.MinValue] = base.Resolve(context);
                dictionary.Add(ComponentActivator, instanceTaskMap);
            }
            else
            {
                Object instance;
                if (!instanceTaskMap.TryGetValue(Task.CurrentId ?? Int32.MinValue, out instance))
                {
                    instance = base.Resolve(context);
                    instanceTaskMap[Task.CurrentId ?? Int32.MinValue] = instance;
                }
            }

            return instanceTaskMap[Task.CurrentId ?? Int32.MinValue];
        }

        /// <summary>
        /// Release the component instance based on the lifestyle semantic.
        /// </summary>
        public override bool Release(object instance)
        {
            // Do nothing.
            return false;
        }
    }
}
