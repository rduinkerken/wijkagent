using Find_My_Boef.Controller;
using Find_My_Boef.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ToastNotifications.Messages;

namespace Find_My_Boef.DataContext
{
    public class OfficerDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        public ObservableCollection<Officer> Officers { get; set; } = new();

        #region Coop Officer Information
        public int CoopOfficerId { get; set; }
        public string CoopOfficerFullName { get; set; }
        #endregion

        #region Officer Information
        public static int OfficerId { get; set; }
        public static string OfficerFullName { get; set; }
        public static Offense? CurrentOffense { get; set; } // may be null
        #endregion 

        #region WindowSettings
        public int RegularTextFontSize { get; set; } = int.Parse(ConfigurationManager.AppSettings.Get("Regular_Text_Font_Size"));
        public FontFamily FontFamily { get; set; } = new("Calibri");
        public string WindowTitle { get; set; }
        #endregion

        public static bool IsExistingOfficerId(int officerId)
        {
            string query = "SELECT * FROM Werknemers WHERE Werknemersnummer = @officerId";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter officerIdParam = new("@officerId", System.Data.SqlDbType.Int);
            officerIdParam.Value = officerId;
            command.Parameters.Add(officerIdParam);
            command.Prepare();
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                reader.Close();
                return true;
            }
            else
            {
                reader.Close();
                return false;
            }
        }

        public void CreateOfficerInformationWindow(int officerId)
        {
            if (!IsExistingOfficerId(officerId))
            {
                MapWindow.Notifier.ShowError($"Werknemer met werknemersnummer {officerId} bestaat niet.");
                return;
            }
            RetrieveOfficerInformation(officerId);
            //NotifyPropertyChanged();
        }

        /// <summary>
        ///  officerId represents the officer which you want to know the coop-officer of.
        /// </summary>
        /// <param name="officerId"></param>
        public void RetrievePartnerInformation(int officerId)
        {
            string query = "SELECT Partnernummer FROM Wijkagent WHERE Werknemersnummer = @officerId";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter officerIdParam = new("@officerId", System.Data.SqlDbType.Int);
            officerIdParam.Value = officerId;
            command.Parameters.Add(officerIdParam);
            command.Prepare();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                CoopOfficerId = 0;
                while (reader.Read())
                {
                    CoopOfficerId = reader.GetInt32(0);
                }
            }
            if (!IsExistingOfficerId(CoopOfficerId))
            {
                CoopOfficerFullName = "Geen partner";
                return;
            }
            string coopOfficerQuery = "SELECT Voornaam, Tussenvoegsel, Achternaam FROM Werknemers WHERE Werknemersnummer = @coopOfficerId";
            SqlCommand coopOfficerCommand = new(coopOfficerQuery, Database.Connection);
            SqlParameter coopOfficerIdParam = new("@coopOfficerId", System.Data.SqlDbType.Int);
            coopOfficerIdParam.Value = CoopOfficerId;
            coopOfficerCommand.Parameters.Add(coopOfficerIdParam);
            coopOfficerCommand.Prepare();
            using (SqlDataReader reader = coopOfficerCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    CoopOfficerFullName = reader.GetString(0) + " " + reader.GetString(1) + " " + reader.GetString(2);
                }
            }
        }

        public void RetrieveOfficerInformation(int officerId)
        {
            string retrieveOfficerInformationQuery = "SELECT Voornaam, Tussenvoegsel, Achternaam FROM Werknemers WHERE Werknemersnummer = @officerId";
            SqlCommand selectOfficerInformationCommand = new(retrieveOfficerInformationQuery, Database.Connection);
            SqlParameter officerIdParam = new("@officerId", System.Data.SqlDbType.Int);
            officerIdParam.Value = officerId;
            selectOfficerInformationCommand.Parameters.Add(officerIdParam);
            selectOfficerInformationCommand.Prepare();
            using (SqlDataReader reader = selectOfficerInformationCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    OfficerId = officerId;
                    OfficerFullName = reader.GetString(0) + " " + reader.GetString(1) + " " + reader.GetString(2);
                    WindowTitle = $"Werknemer: {OfficerFullName} ({OfficerId})";
                }
            }
            RetrievePartnerInformation(officerId);
        }

        public void LoadData()
        {
            Officers.Clear();
            string query = "SELECT W.Werknemersnummer, Voornaam, Tussenvoegsel, Achternaam\r\nFROM Wijkagent W\r\nLEFT JOIN Werknemers W2 on W.Werknemersnummer = W2.Werknemersnummer\r\nWHERE W.Werknemersnummer != @Werknemersnummer";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter employeeNumParam = new("@Werknemersnummer", SqlDbType.Int);
            employeeNumParam.Value = OfficerId;
            command.Parameters.Add(employeeNumParam);
            command.Prepare();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Officers.Add(new Officer()
                    {
                        OfficerId = reader.GetInt32(0),
                        FullName = reader.GetString(1) + " " + reader.GetString(2) + " " + reader.GetString(3)
                    });
                }
            }

            NotifyPropertyChanged();
        }

        public void SearchAgents(string searchText)
        {
            string query =
                @"SELECT W.Werknemersnummer, Voornaam, Tussenvoegsel, Achternaam
                FROM Wijkagent W
                LEFT JOIN Werknemers W2 on W2.Werknemersnummer = W.Werknemersnummer
                WHERE REPLACE(CONCAT_WS (' ', Voornaam, Tussenvoegsel, Achternaam), ' ', '') LIKE REPLACE('%" + searchText + @"%', ' ', '')
                AND W.Werknemersnummer != @Werknemersnummer";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter employeeNumParam = new("@Werknemersnummer", SqlDbType.Int);
            employeeNumParam.Value = OfficerId;
            command.Parameters.Add(employeeNumParam);
            command.Prepare();
            Officers.Clear();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Officers.Add(new Officer()
                    {
                        OfficerId = reader.GetInt32(0),
                        FullName = reader.GetString(1) + " " + reader.GetString(2) + " " + reader.GetString(3)
                    });
                }
            }
        }

        public void RemovePartner()
        {
            string query = "UPDATE Wijkagent\r\nSET Partnernummer=0\r\nWHERE Partnernummer=@Partnernummer AND Werknemersnummer=@Werknemersnummer";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter partnerParam = new("@Partnernummer", SqlDbType.Int);
            partnerParam.Value = CoopOfficerId;
            command.Parameters.Add(partnerParam);
            SqlParameter employeeNumParam = new("@Werknemersnummer", SqlDbType.Int);
            employeeNumParam.Value = OfficerId;
            command.Parameters.Add(employeeNumParam);
            command.Prepare();
            CoopOfficerFullName = "Geen partner";
            CoopOfficerId = 0;
            command.ExecuteNonQuery();
            PropertyChanged(this, new PropertyChangedEventArgs(""));
        }

        public void AddPartner(int index)
        {
            string query = "UPDATE Wijkagent\r\nSET Partnernummer=@Partnernummer\r\nWHERE Werknemersnummer=@Werknemersnummer";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter employeeNumParam = new("@Werknemersnummer", SqlDbType.Int);
            employeeNumParam.Value = OfficerId;
            command.Parameters.Add(employeeNumParam);
            SqlParameter partnerParam = new("@Partnernummer", SqlDbType.Int);
            partnerParam.Value = Officers[index].OfficerId;
            command.Parameters.Add(partnerParam);
            command.Prepare();
            command.ExecuteNonQuery();

            query =
                "SELECT W.WerknemersNummer, Voornaam, Tussenvoegsel, Achternaam FROM Werknemers W\r\nLEFT JOIN Wijkagent W2 on W.Werknemersnummer = W2.Werknemersnummer\r\nWHERE W.Werknemersnummer = @PartnerNummer";
            command = new(query, Database.Connection);
            employeeNumParam = new("@PartnerNummer", SqlDbType.Int);
            employeeNumParam.Value = Officers[index].OfficerId;
            command.Parameters.Add(employeeNumParam);
            command.Prepare();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    CoopOfficerFullName = reader.GetString(1) + " " + reader.GetString(2) + " " + reader.GetString(3);
                    CoopOfficerId = 0;
                }
            }
            PropertyChanged(this, new PropertyChangedEventArgs(""));
        }
    }
}
