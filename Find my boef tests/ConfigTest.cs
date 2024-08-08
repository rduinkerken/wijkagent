using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Configuration;
using System.IO;

namespace Find_my_boef_tests
{
    public class ConfigTest
    {
        [Test]
        public void Config_CanFetchSetting()
        {
            //Retrieve config
            DirectoryInfo di = new(Directory.GetCurrentDirectory());
            di = di.Parent;
            di = di.Parent;
            di = di.Parent;
            di = di.Parent;
            ExeConfigurationFileMap configMap = new()
            {
                ExeConfigFilename = di.FullName + "\\Find My Boef\\App.config"
            };
            Configuration configFile = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            KeyValueConfigurationCollection settings = configFile.AppSettings.Settings;


            var result = settings["Unit_Test"].Value;

            Assert.IsInstanceOf<string>(result);
            Assert.That(result, Is.EqualTo("Unit test is working"));
        }
    }
}
