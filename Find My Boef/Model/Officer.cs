using Find_My_Boef.Controller;
using Find_My_Boef.View;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Configuration;

namespace Find_My_Boef.Model
{
    public class Officer : IIcon
    {
        public int OfficerId;
        public PointLatLng Location;
        private PatrolRoute _patrolRoute;
        public int PartnerId;
        private DateTime _lastUpdated = DateTime.Now;
        private bool _isDisconnected = true;
        private bool _lastConnectionState = true;
        private readonly int _timeToSuspectDisconnection = int.Parse(ConfigurationManager.AppSettings.Get("Time_To_Suspect_Disconnection"));
        public string FullName { get; set; }
        public GMapMarker Marker { get; set; }

        public Officer(int officerID, PointLatLng location)
        {
            OfficerId = officerID;
            Location = location;
            MainInstance.Timer += MainInstance_Timer;

            _patrolRoute = new();
        }

        public Officer()
        {
            OfficerId = 0;
            Location = PointLatLng.Empty;
            MainInstance.Timer += MainInstance_Timer;
        }

        private void MainInstance_Timer(object? sender, System.Timers.ElapsedEventArgs e)
        {
            int diffInSeconds = (int)(DateTime.Now - _lastUpdated).TotalSeconds;

            // more than x seconds no response
            _isDisconnected = _timeToSuspectDisconnection < diffInSeconds;

            if (_lastConnectionState != _isDisconnected)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    ReDraw();
                });
            }

            _lastConnectionState = _isDisconnected;
        }

        public void RedrawPatrolRoute()
        {
            Visualization.ClearPatrolRoute(_patrolRoute);

            _patrolRoute.Polygon = new(_patrolRoute.Points);

            Visualization.DrawPatrolRoute(_patrolRoute);
        }
        public void Draw()
        {
            Visualization.AddIconToMap(Location, (_isDisconnected) ? Visualization.MarkerImage.OfficerDisconnected : Visualization.MarkerImage.Officer, this, OfficerId);
        }

        public void Remove()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Marker?.Clear();
            });
        }

        public void ReDraw()
        {
            Remove();
            Visualization.AddIconToMap(Location, (_isDisconnected) ? Visualization.MarkerImage.OfficerDisconnected : Visualization.MarkerImage.Officer, this, OfficerId);
        }
        public void ReLocate(PointLatLng pointTo)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if (Marker == null)
                {
                    return;
                }

                _lastUpdated = DateTime.Now;

                Marker.Position = pointTo;
                Location = pointTo;

                _patrolRoute.addPoint(pointTo);

                RedrawPatrolRoute();
            });
        }

        public void SetDisconnected(bool isDisconnected)
        {
            _isDisconnected = isDisconnected;
        }
    }
}