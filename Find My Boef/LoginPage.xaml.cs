using Find_My_Boef.Controller;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace Find_My_Boef
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Window
    {
        private static Notifier s_notifier = new(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.TopRight,
                offsetX: 10,
                offsetY: 10);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(3),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });
        public LoginPage()
        {
            if (!Database.IsOpen)
            {
                Database.Initialize();
            }
            InitializeComponent();
        }

        private static int _employeeNumber;

        private void btnSubmit_Click(object sender, RoutedEventArgs args)
        {
            if (Regex.IsMatch(EmployeeNum.Text, @"\b[0-9]{7}\b"))
            {
                _employeeNumber = int.Parse(EmployeeNum.Text);
            }
            else
            {
                s_notifier.ShowError("De logingegevens waren onjuist!");
                return;
            }
            string query = "SELECT Password, SALT, Voornaam, Tussenvoegsel, Achternaam FROM Werknemers WHERE Werknemersnummer=@Username";

            SqlCommand command = new(query, Database.Connection);
            SqlParameter wnParam = new("@Username", System.Data.SqlDbType.Int);
            wnParam.Value = _employeeNumber;
            command.Parameters.Add(wnParam);
            command.Prepare();
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                string hashedPassword = reader.GetString(0);
                string salt = reader.GetString(1);
                string inputPassword = txtPassword.Password;
                string voornaam = reader.GetString(2);
                string tussenvoegsel = reader.GetString(3);
                string achternaam = reader.GetString(4);
                HashAlgorithm sha = SHA256.Create();
                //Hashed password check
                reader.Close();
                if (Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(String.Concat(inputPassword, salt)))) == hashedPassword)
                {
                    MainInstance.LoggedInEmployee = new Model.Employee(voornaam, tussenvoegsel, achternaam);
                    SessionData.LogIn(_employeeNumber);
                    MapWindow mapWindow = new();
                    mapWindow.Show();
                    Close();
                }
                else
                {
                    s_notifier.ShowError("De logingegevens waren onjuist!");
                }
            }
            else
            {
                s_notifier.ShowError("De logingegevens waren onjuist!");
            }
        }
        private void btnHelp_Click(object sender, RoutedEventArgs args)
        {
            Process.Start(new ProcessStartInfo("https://www.politie.nl/contact") { UseShellExecute = true });
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}