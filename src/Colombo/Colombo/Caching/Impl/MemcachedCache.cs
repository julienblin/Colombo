using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using BeIT.MemCached;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using Castle.Core.Logging;
using Colombo.Alerts;

namespace Colombo.Caching.Impl
{
    public class MemcachedCache : IColomboCache
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private IColomboAlerter[] alerters = new IColomboAlerter[0];
        public IColomboAlerter[] Alerters
        {
            get { return alerters; }
            set
            {
                if (value == null) throw new ArgumentNullException("Alerters");
                Contract.EndContractBlock();

                alerters = value;

                if (Logger.IsInfoEnabled)
                {
                    if (alerters.Length == 0)
                        Logger.Info("No alerters has been registered for the MemcachedCache.");
                    else
                        Logger.InfoFormat("MemcachedCache monitoring with the following alerters: {0}", string.Join(", ", alerters.Select(x => x.GetType().Name)));
                }
            }
        }

        private readonly string[] servers;
        private readonly MemcachedClient memcachedClient;
        private readonly SHA1 sha1 = new SHA1CryptoServiceProvider();

        public MemcachedCache(string serverUri)
            : this(new string[] { serverUri })
        {
        }

        public MemcachedCache(string[] servers)
        {
            this.servers = servers;
            memcachedClient = new MemcachedClient(@"Colombo", servers);
        }

        private string GetEncodedKey(string key)
        {
            return Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(key)));
        }

        private string GetKeyForCurrentIncrement(string segment, string type)
        {
            return GetEncodedKey(string.Format("{0}.{1}.current", segment, type));
        }

        private string GetFinalCacheKey(string segment, string type, string cacheKey)
        {
            var keyForCurrentIncrement = GetKeyForCurrentIncrement(segment, type);
            ulong currentIncrementValue = 0;

            var currentIncrementValueFromMemcache = memcachedClient.Get(keyForCurrentIncrement);
            if (currentIncrementValueFromMemcache != null)
            {
                currentIncrementValue = (ulong)currentIncrementValueFromMemcache;
            }
            else
            {
                memcachedClient.Add(keyForCurrentIncrement, (ulong)0);
            }
            return GetEncodedKey(string.Format("{0}.{1}.{2}.{3}", segment, type, currentIncrementValue, cacheKey));
        }

        public void Store(string segment, string cacheKey, object @object, TimeSpan duration)
        {
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");
            if (@object == null) throw new ArgumentNullException("object");
            if (duration.Equals(TimeSpan.MaxValue)) throw new ArgumentException("duration cannot be TimeSpan.MaxValue");
            Contract.EndContractBlock();

            var finalCachekey = GetFinalCacheKey(segment, @object.GetType().FullName, cacheKey);

            using (var backing = new StringWriter())
            using (var writer = new XmlTextWriter(backing))
            {
                var serializer = new DataContractSerializer(@object.GetType());
                serializer.WriteObject(writer, @object);
                if (!memcachedClient.Set(finalCachekey, backing.ToString(), duration))
                {
                    var alert = new MemcachedUnreachableAlert(Environment.MachineName, servers);
                    Logger.Warn(alert.ToString());
                    foreach (var alerter in Alerters)
                    {
                        alerter.Alert(alert);
                    }
                }
            }
        }

        public object Get(string segment, Type objectType, string cacheKey)
        {
            var finalCachekey = GetFinalCacheKey(segment, objectType.FullName, cacheKey);
            var objectStr = memcachedClient.Get(finalCachekey) as string;
            if (objectStr == null)
                return null;

            using (var backing = new StringReader(objectStr))
            using (var reader = new XmlTextReader(backing))
            {
                var serializer = new DataContractSerializer(objectType);
                return serializer.ReadObject(reader);
            }
        }

        public void Flush(string segment, Type objectType)
        {
            if (objectType == null) throw new ArgumentNullException("objectType");
            Contract.EndContractBlock();

            var keyForCurrentIncrement = GetKeyForCurrentIncrement(segment, objectType.FullName);
            memcachedClient.Increment(keyForCurrentIncrement, 1);
        }

        public void FlushAll()
        {
            memcachedClient.FlushAll();
        }
    }
}
