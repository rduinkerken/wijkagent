using Find_My_Boef.Model;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Find_My_Boef.Controller
{
    public class CameraData
    {
        public static List<Camera> LoadCameras(bool drawIcon)
        {
            List<Camera> returnValue = new();
            string query = "SELECT Locatie, Status, URL, Id FROM Camera";
            SqlCommand command = new(query, Database.Connection);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                string pointString = reader.GetString(0);
                String[] separator = { ";" };
                String[] points = pointString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                double lat = double.Parse(points[0], System.Globalization.CultureInfo.InvariantCulture);
                double lng = double.Parse(points[1], System.Globalization.CultureInfo.InvariantCulture);
                Camera newCamera = new Camera(new PointLatLng(lat, lng), (CameraStatus)reader.GetByte(1), reader.GetString(2), reader.GetInt32(3));
                if (drawIcon) newCamera.Draw();
                returnValue.Add(newCamera);
            }

            return returnValue;
        }
    }
}
