using Renci.SshNet;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Find_My_Boef.Controller
{
    public static class Database
    {
        public static SqlConnection Connection { get; set; } = new();
        public static bool IsOpen { get; set; }

        // This function initializes the connection with the database, and sets the Connection variable
        public static void Initialize()
        {
            string SSHHost = ConfigurationManager.AppSettings.Get("SSH_Host");
            string SSHUser = ConfigurationManager.AppSettings.Get("SSH_Naam");
            string SSHPassword = ConfigurationManager.AppSettings.Get("SSH_Wachtwoord");
            string DatabaseHost = ConfigurationManager.AppSettings.Get("Database_Host");
            int DatabasePort = int.Parse(ConfigurationManager.AppSettings.Get("Database_Poort")) - 1;
            string DatabaseUser = ConfigurationManager.AppSettings.Get("Database_Gebruiker");
            string DatabasePassword = ConfigurationManager.AppSettings.Get("Database_Wachtwoord");
            string DatabaseName = ConfigurationManager.AppSettings.Get("Database_Naam");
            PasswordConnectionInfo connectionInfo = new(SSHHost, SSHUser, SSHPassword);
            IsOpen = true;
            connectionInfo.Timeout = TimeSpan.FromSeconds(30);
            SshClient client = new(connectionInfo);
            client.Connect();

            while (Connection.State == ConnectionState.Closed)
            {
                DatabasePort++;
                try
                {
                    ForwardedPortLocal portForward = new("127.0.0.1", Convert.ToUInt32(DatabasePort), DatabaseHost, Convert.ToUInt32(DatabasePort));
                    client.AddForwardedPort(portForward);
                    portForward.Start();
                    SqlConnectionStringBuilder builder = new()
                    {
                        DataSource = "127.0.0.1",
                        UserID = DatabaseUser,
                        Password = DatabasePassword,
                        InitialCatalog = DatabaseName,
                        MultipleActiveResultSets = true
                    };
                    Connection = new SqlConnection(builder.ConnectionString);
                    Connection.Open();
                }
                catch (SqlException e)
                {
                    Debug.WriteLine(e.ToString());
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }
        }
    }
}
