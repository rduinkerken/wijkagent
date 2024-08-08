using Find_My_Boef.Controller;
using SocketServer;
using System.Configuration;
using WebSocketSharp.Server;

namespace Server
{
    public class Program
    {
        public static void Main()
        {
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
            WebSocketServer wssv = StartSocketServer();
            Database.Initialize();

            Console.WriteLine("\nPress Enter key to stop the server...");
            Console.ReadLine();

            wssv.Stop();
        }

        private static WebSocketServer StartSocketServer()
        {
            // creates a websocket server
            WebSocketServer wssv = new(System.Net.IPAddress.Any, 6969);

            // adds the service that will listen and parse data to all connected clients
            wssv.AddWebSocketService<Parser>("/Parser");

            // start server
            wssv.Start();

            if (wssv.IsListening)
            {
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", wssv.Port);

                foreach (var path in wssv.WebSocketServices.Paths)
                {
                    Console.WriteLine("- {0}", path);
                }
            }

            return wssv;
        }
    }
}