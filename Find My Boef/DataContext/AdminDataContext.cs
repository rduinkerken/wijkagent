using Find_My_Boef.Controller;
using Find_My_Boef.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

namespace Find_My_Boef.DataContext
{
    public class AdminDataContext : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public static int RegularTextFontSize { get; set; } = int.Parse(ConfigurationManager.AppSettings.Get("Regular_Text_Font_Size"));

        public ObservableCollection<Administrator> Administrators { get; set; } = new();

        public ObservableCollection<Employee> EmployeeListAdmin { get; set; } = new();

        public ObservableCollection<Employee> EmployeeListOfficer { get; set; } = new();

        public ObservableCollection<Officer> Officers { get; set; } = new();
        public ObservableCollection<Session> Sessions { get; set; } = new();

        public void LoadData()
        {
            Administrators.Clear();
            EmployeeListAdmin.Clear();
            EmployeeListOfficer.Clear();
            Officers.Clear();
            Sessions.Clear();
            string query = "SELECT A.Werknemersnummer, W.Voornaam, W.Tussenvoegsel, W.Achternaam FROM Administrator A\r\nLEFT JOIN Werknemers W ON A.Werknemersnummer = W.Werknemersnummer";
            SqlCommand command = new(query, Database.Connection);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Administrators.Add(new Administrator()
                    {
                        EmployeeNumber = reader.GetInt32(0),
                        FullName = reader.GetString(1) + " " + reader.GetString(2) + " " + reader.GetString(3)
                    });
                }
            }
            query = "SELECT Werknemersnummer, Voornaam, Tussenvoegsel, Achternaam FROM Werknemers W\r\nWHERE W.Werknemersnummer NOT IN (SELECT A.Werknemersnummer FROM Administrator A)";
            command = new(query, Database.Connection);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Employee employee = new(reader.GetString(1), reader.GetString(2), reader.GetString(3))
                    {
                        EmployeeNumber = reader.GetInt32(0)
                    };
                    EmployeeListAdmin.Add(employee);
                }
            }
            query = "SELECT W.Werknemersnummer, Voornaam, Tussenvoegsel, Achternaam\r\nFROM Werknemers W\r\nWHERE W.Werknemersnummer NOT IN (SELECT W2.Werknemersnummer FROM Wijkagent W2)";
            command = new(query, Database.Connection);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Employee employee = new(reader.GetString(1), reader.GetString(2), reader.GetString(3))
                    {
                        EmployeeNumber = reader.GetInt32(0)
                    };
                    EmployeeListOfficer.Add(employee);
                }
            }
            query = "SELECT W.Werknemersnummer, Voornaam, Tussenvoegsel, Achternaam\r\nFROM Wijkagent W\r\nLEFT JOIN Werknemers W2 on W.Werknemersnummer = W2.Werknemersnummer";
            command = new(query, Database.Connection);
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
            query = "SELECT S.SessieId, Voornaam, Tussenvoegsel, Achternaam FROM Sessie S\r\nLEFT JOIN Werknemers W on S.Werknemersnummer = W.Werknemersnummer";
            command = new(query, Database.Connection);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Sessions.Add(new Session()
                    {
                        SessionId = reader.GetInt32(0),
                        FullName = reader.GetString(1) + " " + reader.GetString(2) + " " + reader.GetString(3)
                    });
                }
            }
            NotifyPropertyChanged();
        }

        public void SearchEmployeesAdmin(string searchText)
        {
            string query =
                "SELECT Werknemersnummer, Voornaam, Tussenvoegsel, Achternaam\r\nFROM Werknemers\r\nWHERE REPLACE (CONCAT_WS (' ', Voornaam, Tussenvoegsel, Achternaam), ' ', '') LIKE REPLACE('%" + searchText + "%', ' ', '')";
            SqlCommand command = new(query, Database.Connection);
            command.Prepare();
            EmployeeListAdmin.Clear();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Employee employee = new(reader.GetString(1), reader.GetString(2), reader.GetString(3))
                    {
                        EmployeeNumber = reader.GetInt32(0)
                    };
                    EmployeeListAdmin.Add(employee);
                }
            }
        }

        public void SearchEmployeesOfficer(string searchText)
        {
            string query =
                "SELECT Werknemersnummer, Voornaam, Tussenvoegsel, Achternaam\r\nFROM Werknemers\r\nWHERE REPLACE (CONCAT_WS (' ', Voornaam, Tussenvoegsel, Achternaam), ' ', '') LIKE REPLACE('%" + searchText + "%', ' ', '')";
            SqlCommand command = new(query, Database.Connection);
            command.Prepare();
            EmployeeListOfficer.Clear();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Employee employee = new(reader.GetString(1), reader.GetString(2), reader.GetString(3))
                    {
                        EmployeeNumber = reader.GetInt32(0)
                    };
                    EmployeeListOfficer.Add(employee);
                }
            }
        }

        public void AddAdmin(int index)
        {
            string query = "INSERT INTO Administrator VALUES (@Werknemersnummer)";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter employeeNumParam = new("@Werknemersnummer", System.Data.SqlDbType.Int);
            employeeNumParam.Value = EmployeeListAdmin[index].EmployeeNumber;
            command.Parameters.Add(employeeNumParam);
            command.Prepare();
            command.ExecuteNonQuery();
            LoadData();
        }

        public void RemoveAdmin(int index)
        {
            string query = "DELETE FROM Administrator WHERE Werknemersnummer = @Werknemersnummer";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter employeeNumParam = new("@Werknemersnummer", System.Data.SqlDbType.Int);
            employeeNumParam.Value = Administrators[index].EmployeeNumber;
            command.Parameters.Add(employeeNumParam);
            command.Prepare();
            command.ExecuteNonQuery();
            LoadData();
        }

        public void AddOfficer(int index)
        {
            string query = "INSERT INTO Locatie (Werknemersnummer)\r\nVALUES (@Werknemersnummer)\r\nINSERT INTO Wijkagent(Werknemersnummer)\r\nVALUES (@Werknemersnummer)";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter employeeNumParam = new("@Werknemersnummer", System.Data.SqlDbType.Int);
            employeeNumParam.Value = EmployeeListOfficer[index].EmployeeNumber;
            command.Parameters.Add(employeeNumParam);
            command.Prepare();
            command.ExecuteNonQuery();

            query = "INSERT INTO WijkagentDelict VALUES (@Wijkagentnummer, 0)";
            command = new(query, Database.Connection);
            employeeNumParam = new("@Wijkagentnummer", System.Data.SqlDbType.Int);
            employeeNumParam.Value = EmployeeListOfficer[index].EmployeeNumber;
            command.Parameters.Add(employeeNumParam);
            command.Prepare();
            command.ExecuteNonQuery();
            LoadData();
        }

        public void RemoveOfficer(int index)
        {
            string query = "DELETE FROM Locatie\r\nWHERE Werknemersnummer = @Werknemersnummer\r\nDELETE FROM Wijkagent\r\nWHERE Werknemersnummer = @Werknemersnummer";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter employeeNumParam = new("@Werknemersnummer", System.Data.SqlDbType.Int);
            employeeNumParam.Value = Officers[index].OfficerId;
            command.Parameters.Add(employeeNumParam);
            command.Prepare();
            command.ExecuteNonQuery();
            LoadData();
        }

        public void RemoveSession(int index)
        {
            string query = "DELETE FROM Sessie WHERE SessieId = @SessieId";
            SqlCommand command = new(query, Database.Connection);
            SqlParameter sessionIdParam = new("@SessieId", System.Data.SqlDbType.Int);
            sessionIdParam.Value = Sessions[index].SessionId;
            command.Parameters.Add(sessionIdParam);
            command.Prepare();
            command.ExecuteNonQuery();
            LoadData();
        }

        public void SearchSession(string searchText)
        {
            string query = @"SELECT W.Werknemersnummer, Voornaam, Tussenvoegsel, Achternaam
                            FROM Werknemers W
                            LEFT JOIN Sessie S on W.Werknemersnummer = S.Werknemersnummer
                            WHERE REPLACE (CONCAT_WS (' ', Voornaam, Tussenvoegsel, Achternaam), ' ', '') LIKE REPLACE('%" + searchText + "%', ' ', '')";
            SqlCommand command = new(query, Database.Connection);
            command.Prepare();
            Sessions.Clear();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Sessions.Add(new Session()
                    {
                        SessionId = reader.GetInt32(0),
                        FullName = reader.GetString(1) + " " + reader.GetString(2) + " " + reader.GetString(3)
                    });
                }
            }
        }
    }
}
