using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics.Contracts;

namespace Colombo.Configuration
{
    public class ColomboConfigurationSection : ConfigurationSection, IColomboConfiguration
    {
        public const string SectionName = @"colombo";

        [ConfigurationProperty("endpoints")]
        public EndPointsConfigurationCollection EndPoints
        {
            get
            {
                return (EndPointsConfigurationCollection)this["endpoints"];
            }
            set
            {
                this["endpoints"] = value;
            }
        }

        public string GetTargetAddressFor(BaseRequest request, string targetType)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (targetType == null) throw new ArgumentNullException("targetType");
            Contract.EndContractBlock();

            var assemblyName = request.GetType().Assembly.GetName().Name;

            var selectedEndPoints = new List<EndPointConfigurationElement>();

            if (EndPoints != null)
            {
                foreach (EndPointConfigurationElement endPoint in EndPoints)
                {
                    if (endPoint.AssemblyName.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase)
                        && endPoint.EndPointType.Equals(targetType, StringComparison.InvariantCultureIgnoreCase)
                        )
                    {
                        selectedEndPoints.Add(endPoint);
                    }
                }
            }

            if (selectedEndPoints.Count == 1)
                return selectedEndPoints[0].Address;

            if (selectedEndPoints.Count > 1)
            {
                throw new ColomboException(
                    string.Format("Duplicate configuration entry for assembly {0} and type {2}: found {3}",
                        assemblyName,
                        targetType,
                        string.Join(", ", selectedEndPoints.Select(x => x.Address))
                    )
                );
            }

            return null;
        }
    }
}
