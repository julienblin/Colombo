using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics.Contracts;

namespace Colombo.Configuration
{
    [ConfigurationCollection(typeof(EndPointConfigurationElement), AddItemName = "endpoint", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class EndPointsConfigurationCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "endpoint"; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new EndPointConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var configElement = element as EndPointConfigurationElement;
            if(configElement == null)
                throw new ColomboException("Internal error: element should not be null");

            Contract.Assume(configElement.AssemblyName != null);
            return configElement.AssemblyName;
        }
    }
}
