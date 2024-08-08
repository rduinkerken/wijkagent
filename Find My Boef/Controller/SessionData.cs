using System;
using System.Data.SqlClient;
using System.Linq;

namespace Find_My_Boef.Controller
{
    public class SessionData
    {
        public static int SessionId;
        public static int SessionOfficerId;
        public static bool IsAdmin = true;
        public static string AuthKey;

        // Checks if user is still allowed to be logged in
        public static bool IsLoggedIn()
        {
            // Delete old sessions
            string query = "DELETE FROM Sessie WHERE EindDatum<GETDATE()";
            SqlCommand command = new(query, Database.Connection);
            command.ExecuteNonQuery();

            //Find new sessions 
            query = "SELECT * FROM Sessie WHERE SessieId=@SessieId";
            command = new SqlCommand(query, Database.Connection);
            SqlParameter sessionIdParam = new("@SessieId", System.Data.SqlDbType.Int);
            sessionIdParam.Value = SessionId;
            command.Parameters.Add(sessionIdParam);
            command.Prepare();
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                return true;
            }
            SessionId = 0;
            IsAdmin = false;
            return false;
        }

        // Logs in the user and creates a session
        public static void LogIn(int werknemersnummer)
        {
            SessionOfficerId = werknemersnummer;

            // Delete old sessions
            string query = "DELETE FROM Sessie WHERE EindDatum<GETDATE()";
            SqlCommand command = new(query, Database.Connection);
            command.ExecuteNonQuery();

            // Make new session
            query = "INSERT INTO Sessie VALUES (@SessieId, @EindDatum, @Werknemersnummer)";
            command = new SqlCommand(query, Database.Connection);
            SqlParameter sessionIdParam = new("@SessieId", System.Data.SqlDbType.Int);
            SqlParameter endDateParam = new("@EindDatum", System.Data.SqlDbType.DateTime);
            SqlParameter employeeNumParam = new("@Werknemersnummer", System.Data.SqlDbType.Int);
            SessionId = GenerateSessionId();
            sessionIdParam.Value = SessionId;
            endDateParam.Value = DateTime.Now.AddDays(7);
            employeeNumParam.Value = werknemersnummer;
            command.Parameters.Add(sessionIdParam);
            command.Parameters.Add(endDateParam);
            command.Parameters.Add(employeeNumParam);
            command.Prepare();
            command.ExecuteNonQuery();

            // Check administrator
            query = "SELECT * FROM Administrator WHERE Werknemersnummer=@Werknemersnummer";
            command = new SqlCommand(query, Database.Connection);
            employeeNumParam = new("@Werknemersnummer", System.Data.SqlDbType.Int);
            employeeNumParam.Value = werknemersnummer;
            command.Parameters.Add(employeeNumParam);
            command.Prepare();
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                IsAdmin = true;
            }

            // Set authkey for socketserver
            query = "SELECT AuthKey FROM SocketServerAuth WHERE Type=0";
            command = new SqlCommand(query, Database.Connection);
            command.Prepare();
            reader = command.ExecuteReader();
            if (reader.Read())
            {
                AuthKey = reader.GetString(0);
            }
        }

        // Logs out the user and ends the current session
        public static void LogOut()
        {
            // Delete old sessions
            string query = "DELETE FROM Sessie WHERE EindDatum<GETDATE()";
            SqlCommand command = new(query, Database.Connection);
            command.ExecuteNonQuery();

            // Delete current session
            query = "DELETE FROM Sessie WHERE SessieId=@SessieId";
            command = new SqlCommand(query, Database.Connection);
            SqlParameter sessionIdParam = new("@SessieId", System.Data.SqlDbType.Int);
            sessionIdParam.Value = SessionId;
            SessionId = 0;
            command.Parameters.Add(sessionIdParam);
            command.Prepare();
            command.ExecuteNonQuery();
        }

        // Generates a code 9 digits long
        public static int GenerateSessionId()
        {
            const string chars = "0123456789";
            return int.Parse(new string(Enumerable.Repeat(chars, 9)
                .Select(s => s[MainInstance.MainRandom.Next(s.Length)]).ToArray()));
        }
    }
}
