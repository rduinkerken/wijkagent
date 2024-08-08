using Find_My_Boef.Model;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Find_My_Boef.Controller
{
    public static class OffenseData
    {
        // Method to get all offenses and draw them on the map
        public static List<Offense> LoadOffenses(bool drawIcon)
        {
            List<Offense> returnValue = new();
            //Find new sessions
            string query = "SELECT ID, DatumTijd, Locatie, Type, Beschrijving, Status FROM Delict" + (drawIcon ? " WHERE Status != 2" : " ORDER BY DatumTijd");
            SqlCommand command = new(query, Database.Connection);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string pointString = reader.GetString(2);
                String[] separator = { ";" };
                String[] points = pointString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                double lat = double.Parse(points[0], System.Globalization.CultureInfo.InvariantCulture);
                double lng = double.Parse(points[1], System.Globalization.CultureInfo.InvariantCulture);
                Offense newOffense = new Offense(reader.GetInt32(0), (OffenseType)reader.GetByte(3), reader.GetDateTime(1), reader.GetString(4), (Status)reader.GetByte(5), new PointLatLng(lat, lng));
                if (drawIcon) newOffense.Draw();
                returnValue.Add(newOffense);
            }
            return returnValue;
        }

        public static List<int> LoadFilteredOffenses(string QueryArgs)
        {
            List<int> returnValue = new List<int>();
            //Find ID's with filter query
            string Query = "SELECT ID FROM Delict";
            Query += QueryArgs;
            SqlCommand command = new SqlCommand(Query, Database.Connection);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                returnValue.Add(reader.GetInt32(0));
            }
            return returnValue;
        }
    }
}
