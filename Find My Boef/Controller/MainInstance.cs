using Find_My_Boef.Model;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Timers;

namespace Find_My_Boef.Controller
{
    public static class MainInstance
    {
        private static GMapControl s_control = new();
        private static List<Offense> s_offenses = new();
        private static List<Officer> s_officers = new();
        private static List<Camera> s_cameras = new();
        private static readonly int s_disconnectedCheck = int.Parse(ConfigurationManager.AppSettings.Get("Time_To_Suspect_Disconnection"));
        public static List<PointLatLng> NeighbourhoodBounds = new();
        public static event ElapsedEventHandler? Timer;
        public static Random MainRandom = new(DateTime.Now.Millisecond);
        public static Client Client = new(ConfigurationManager.AppSettings.Get("Web_Sock_Url"));
        public static Employee? LoggedInEmployee;

        // Setter and getter for control
        #region Control
        public static void SetControl(GMapControl gMapControl)
        {
            s_control = gMapControl;
        }
        public static GMapControl GetControl()
        {
            return s_control;
        }
        #endregion

        // Event timer initializer
        public static void SetEventTimer()
        {
            Timer aTimer = new();
            aTimer.Elapsed += OnTimeElapsed;
            aTimer.Interval = double.Parse(ConfigurationManager.AppSettings["Timer_Interval"]);
            aTimer.Enabled = true;
            aTimer.Start();
        }

        // Timer actions
        private static void OnTimeElapsed(object? sender, ElapsedEventArgs e)
        {
            Timer?.Invoke(sender, e);
        }

        // Setter for the list neighbourhoodbounds
        public static void SetBounds(List<PointLatLng> points)
        {
            NeighbourhoodBounds = points;
        }

        // Setters and getters for offenses, with reload-, add-, and delete-functions
        #region Offenses
        public static void AddOffense(Offense offense)
        {
            s_offenses.Add(offense);
        }
        public static Offense GetOffensesFromID(int id)
        {
            foreach (Offense offense in s_offenses)
            {
                if (offense.ID == id)
                {
                    return offense;
                }
            }

            return null;
        }
        public static void DeleteOffense(Offense offense)
        {
            s_offenses.Remove(offense);
        }

        public static List<Offense> GetOffenses()
        {
            return s_offenses;
        }

        public static void SetOffenses(List<Offense> offenses)
        {
            s_offenses = offenses;
        }

        public static void ReloadOffenses()
        {
            foreach (Offense o in s_offenses)
            {
                o.Remove();
            }
            s_offenses = OffenseData.LoadOffenses(true);
        }
        public static void ReloadCameras()
        {
            foreach (Camera c in s_cameras)
            {
                c.Remove();
            }
            s_cameras = CameraData.LoadCameras(true);
        }
        #endregion

        // Same as offenses
        #region Officers
        public static void AddOfficer(Officer officer)
        {
            s_officers.Add(officer);
        }
        public static void DeleteOfficer(Officer officer)
        {
            s_officers.Remove(officer);
        }
        public static Officer GetOfficerFromId(int id)
        {
            foreach (Officer o in s_officers)
            {
                if (o.OfficerId == id)
                {
                    return o;
                }
            }

            return null;
        }

        public static void ReloadOfficers()
        {
            foreach (Officer o in s_officers)
            {
                o.Remove();
            }

            s_officers.Clear();
            string query =
                "SELECT Werknemersnummer, DATEDIFF(second, LaatsteUpdate, GETDATE()) , HuidigeLocatie\r\nFROM Locatie";
            SqlCommand command = new(query, Database.Connection);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string pointString = reader.GetString(2);
                String[] separator = { ";" };
                String[] points = pointString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                double lat = double.Parse(points[0], System.Globalization.CultureInfo.InvariantCulture);
                double lng = double.Parse(points[1], System.Globalization.CultureInfo.InvariantCulture);
                Officer o = new(reader.GetInt32(0), new PointLatLng(lat, lng));
                o.SetDisconnected(reader.GetInt32(1) > s_disconnectedCheck);
                o.Draw();
                s_officers.Add(o);
            }
        }
        #endregion

        // Add, delete and getter functions for cameras
        #region Cameras
        public static void AddCamera(Camera camera)
        {
            s_cameras.Add(camera);
        }
        public static void DeleteCamera(Camera camera)
        {
            s_cameras.Remove(camera);
        }
        public static Camera GetCameraFromId(int id)
        {
            foreach (Camera c in s_cameras)
            {
                if (c.CameraId == id)
                {
                    return c;
                }
            }

            return null;
        }
        #endregion
    }
}
