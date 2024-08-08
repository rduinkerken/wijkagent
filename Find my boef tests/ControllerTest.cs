using Find_My_Boef.Controller;
using Find_My_Boef.Model;
using GMap.NET;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Tweetinvi.Models;

namespace Find_my_boef_tests
{
    public class Tests
    {
        private Client _client;

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

            _client = new(ConfigurationManager.AppSettings.Get("Web_Sock_Url"));
            Offense o1 = new(1, OffenseType.Accident, new DateTime(), "test", Status.InProgress, new PointLatLng(20, 20));
            List<Offense> offense = new() { o1 };
            MainInstance.SetOffenses(offense);
        }

        [Test]
        public void Client_IsNotNull()
        {
            Assert.IsNotNull(_client);
        }
        [Test]
        [Apartment(ApartmentState.STA)]
        public void Client_CanConnect()
        {
            // socket server has to be on
            _client.SocketConnect();

            // andere thread dus lange timer
            Thread.Sleep(2500);
            Assert.IsTrue(_client.IsConnected());
        }
        [Test]
        public void MainInstance_SetOffense_HasList()
        {
            Assert.IsInstanceOf<List<Offense>>(MainInstance.GetOffenses());
            Assert.IsNotNull(MainInstance.GetOffenses());
            Assert.IsTrue(MainInstance.GetOffenses().Count == 1);
        }
        [Test]
        public void MainInstance_GetOffensesFromId_ReturnsOffense()
        {
            var result = MainInstance.GetOffensesFromID(1);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Offense>(result);
        }
        [Test]
        public void MainInstance_DeleteOffense_Deletes()
        {
            Offense o1 = MainInstance.GetOffensesFromID(1);
            MainInstance.DeleteOffense(o1);

            Assert.IsTrue(MainInstance.GetOffenses().Count == 0);
        }
        [Test]
        public void Helper_FromArgb_ReturnsSolidColorBrush()
        {
            var result = Helper.FromArgb(255, 255, 0, 0);

            Assert.IsInstanceOf<SolidColorBrush>(result);
        }

        [Test]
        public void Helper_RandomColor_ReturnsRandomColor()
        {
            bool isSame = true;
            SolidColorBrush last = null;

            for (int i = 0; i < 50; i++)
            {
                var result = Helper.RandomColor();

                Assert.IsInstanceOf<SolidColorBrush>(result);

                if (last != null && last != result)
                {
                    isSame = false;
                }
                last = result;
            }
            Assert.IsFalse(isSame);
        }

        [Test]
        public void Get_Correct_Marker()
        {
            OffenseType offenseAccident = OffenseType.Accident;
            OffenseType offenseSubstance = OffenseType.Drugs;
            OffenseType offenseAssault = OffenseType.Assault;
            OffenseType offenseUnknown = OffenseType.Unknown;

            Assert.That(Offense.MarkerImageFromType(offenseAccident) == Find_My_Boef.View.Visualization.MarkerImage.Accident);
            Assert.That(Offense.MarkerImageFromType(offenseSubstance) == Find_My_Boef.View.Visualization.MarkerImage.Substance);
            Assert.That(Offense.MarkerImageFromType(offenseAssault) == Find_My_Boef.View.Visualization.MarkerImage.Assault);
            Assert.That(Offense.MarkerImageFromType(offenseUnknown) == Find_My_Boef.View.Visualization.MarkerImage.Unknown);

        }

        [Test]
        // TODO: DB mensen moeten even kijken of hierbij connectie mogelijk is,
        public void OffenseData_LoadOffenses()
        {

            List<Offense> offenses = OffenseData.LoadOffenses(false);

            Assert.IsNotNull(offenses);

            foreach (Offense o in offenses)
            {
                Assert.IsInstanceOf<Offense>(o);
            }
            Assert.IsTrue(offenses.Count > 0);

        }

        [Test]
        //Genereert sessie ID
        public void SessionData_GenerateSessionId()
        {
            var Id = SessionData.GenerateSessionId();
            Assert.IsNotNull(Id);
        }

        [Test]
        //Test of de tweets worden opgehaald
        public void SocialMediaData_GetSocialMediaData()
        {
            SocialMediaData SMD = new SocialMediaData();
            var smdData = SMD.GetSocialMediaData(52.510338, 6.086134);
            Assert.IsNotNull(smdData);


        }

        [Test]
        //TODO: fix object reference
        public async Task SocialMediaData_CheckTweetRelevance()
        {
            SocialMediaData SMD = new();
            IAsyncEnumerable<SocialMediaPost> posts = SMD.GetSocialMediaData(52.510338, 6.086134);
            if (posts != null)
            {
                await foreach (SocialMediaPost post in posts) //<---- object reference is null?
                {
                    Assert.IsTrue(SMD.CheckPostRelevance(post));
                }
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}