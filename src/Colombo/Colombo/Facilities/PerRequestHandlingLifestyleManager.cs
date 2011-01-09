using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Lifestyle;
using System.Runtime.Serialization;
using System.Threading;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using System.Threading.Tasks;

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

        public static Dictionary<IComponentActivator, IDictionary<int?, object>> Map
        {
            get
            {
                if (map == null)
                {
                    map = new Dictionary<IComponentActivator, IDictionary<int?, object>>();
                }
                return map;
            }
        }

        public override void Dispose()
        {
            foreach (var instance in Map.Values)
            {
                base.Release(instance);
            }
        }

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

        public override bool Release(object instance)
        {
            // Do nothing.
            return false;
        }
    }
}
