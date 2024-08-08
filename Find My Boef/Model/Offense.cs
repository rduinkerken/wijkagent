using Find_My_Boef.View;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Diagnostics;

namespace Find_My_Boef.Model
{
    public enum OffenseType
    {
        Unknown,
        Assault,
        Drugs,
        Accident,
        Disturbance
    }

    public enum Status
    {
        NotVisited,
        InProgress,
        Processed
    }

    public class Offense : IIcon
    {
        public int ID { get; set; }
        public OffenseType Type { get; set; }
        public DateTime Time { get; set; }
        public PointLatLng Location { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public GMapMarker Marker { get; set; }
        public bool IsDrawn { get; set; } = true;

        public Offense(int id, OffenseType type, DateTime time, string description, Status status, PointLatLng location)
        {
            ID = id;
            Type = type;
            Time = time;
            Location = location;
            Description = description;
            Status = status;
        }
        public void Draw()
        {
            Visualization.AddIconToMap(Location, MarkerImageFromType(Type), this, ID);
            IsDrawn = true;
        }
        public void Remove()
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                IsDrawn = false;
                Marker.Clear();
            });
        }
        public void ReDraw()
        {
            Remove();
            Visualization.AddIconToMap(Location, MarkerImageFromType(Type), this, ID);
        }
        public void ReLocate(PointLatLng pointTo)
        {
            if (Marker != null)
            {
                Marker.Position = pointTo;
            }
        }
        public static Visualization.MarkerImage MarkerImageFromType(OffenseType type)
        {
            return type switch
            {
                OffenseType.Assault => Visualization.MarkerImage.Assault,
                OffenseType.Drugs => Visualization.MarkerImage.Substance,
                OffenseType.Accident => Visualization.MarkerImage.Accident,
                _ => Visualization.MarkerImage.Unknown,
            };
        }

        public static string ReturnDutchEnumValue<T>(T enumerationValue)
        {
            if (enumerationValue is Status)
            {
                switch (enumerationValue)
                {
                    case Status.NotVisited: return "Onbezocht";
                    case Status.InProgress: return "Bezig";
                    case Status.Processed: return "Afgehandeld";
                }
            }
            else if (enumerationValue is OffenseType)
            {
                switch (enumerationValue)
                {
                    case OffenseType.Unknown: return "Onbekend";
                    case OffenseType.Assault: return "Overval";
                    case OffenseType.Drugs: return "Drugs";
                    case OffenseType.Accident: return "Ongeval";
                    case OffenseType.Disturbance: return "Overlast";
                }
            }
            throw new ArgumentException(enumerationValue + " is not a valid argument.");
        }

        // Debug function to print all info of current offense
        public void DebugShowOffense()
        {
            Debug.WriteLine("ID: {0}\nType: {1}\nTime: {2}\nLat: {3}\nLng: {4}\nDescription: {5}\nStatus: {6}", ID, Type, Time, Location.Lat, Location.Lng, Description, Status);
        }
    }
}
