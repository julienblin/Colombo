using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Configuration;
using Colombo.Configuration;
using Rhino.Mocks;
using System.IO;

namespace Colombo.Tests.Configuration
{
    [TestFixture]
    public class ColomboConfigurationSectionTest
    {
        [Test]
        public void It_should_retrieve_correct_values()
        {
            var request = new TestRequest();

            var config = GetConfigurationForFile("Complete.config");
            Assert.That(() => config.GetTargetAddressFor(request, "wcf"),
                Is.EqualTo(@"http://localhost/Colombo.svc"));
            Assert.That(() => config.GetTargetAddressFor(request, "another"),
                Is.Null);
        }

        private IColomboConfiguration GetConfigurationForFile(string fileName)
        {
            var fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = Path.Combine(@"Configuration\Files\", fileName);
            var mappedConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var config= (IColomboConfiguration)mappedConfig.GetSection(ColomboConfigurationSection.SectionName);
            Assert.That(() => config, Is.Not.Null);
            return config;
        }

        public class TestRequest : Request<TestResponse>
        {
        }
    }
}
