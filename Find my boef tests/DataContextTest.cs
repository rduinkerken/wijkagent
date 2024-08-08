using Find_My_Boef.Controller;
using Find_My_Boef.DataContext;
using NUnit.Framework;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using static Find_My_Boef.DataContext.OffenseDataContext;
using static Find_My_Boef.DataContext.OfficerDataContext;

namespace Find_my_boef_tests
{
    public class DataContextTest
    {
        [SetUp]
        public void Setup()
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
            foreach (KeyValueConfigurationElement setting in settings)
            {
                ConfigurationManager.AppSettings.Set(setting.Key, setting.Value);
            }
            Database.Initialize();
        }

        [Test]
        public void OffenseDataContext_IsExistingOffenseId_IsTrue()
        {
            SqlCommand command = new SqlCommand("SELECT TOP 1 ID FROM Delict", Database.Connection);
            SqlDataReader reader = command.ExecuteReader();
            int existingOffenseId = -1;

            if (reader.Read())
            {
                existingOffenseId = reader.GetInt32(0);
            }
            if (existingOffenseId == -1)
            {
                Assert.Fail("No officerId has been read from the database");
                return;
            }
            Assert.IsTrue(IsExistingOffenseId(existingOffenseId));
        }

        [Test]
        public void OfficerDataContext_IsExistingOfficerId_IsTrue()
        {
            SqlCommand command = new SqlCommand("SELECT TOP 1 Werknemersnummer FROM Wijkagent", Database.Connection); 
            SqlDataReader reader = command.ExecuteReader();
            int existingOfficerId = -1; 
             
            if (reader.Read())  
            {
                existingOfficerId = reader.GetInt32(0);
            }
            if (existingOfficerId == -1) 
            {
                Assert.Fail("No officerId has been read from the database");
                return; 
            }
            Assert.IsTrue(IsExistingOfficerId(existingOfficerId));
        }

        [Test]
        // Test if data context gets correct settings
        public void SettingsDataContext_GetDataContext()
        {
            Assert.That(SettingsDataContext.GetDataContext().CurrentHeatMapSetting == double.Parse(ConfigurationManager.AppSettings["Heat_Map_Ratio"]));
            Assert.That(SettingsDataContext.GetDataContext().CurrentZoomValue == int.Parse(ConfigurationManager.AppSettings["Zoom_Value"]));
            Assert.That(SettingsDataContext.GetDataContext().CurrentTimerSetting == int.Parse(ConfigurationManager.AppSettings["Timer_Interval"]) / 1000);
        }

    }
}
