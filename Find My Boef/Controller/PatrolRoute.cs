using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Media;

namespace Find_My_Boef.Controller
{
    public class PatrolRoute
    {
        public Queue<PointLatLng> Points { get; private set; }
        public SolidColorBrush PatrolColor { get; set; }
        public GMapRoute Polygon { get; set; }
        public PatrolRoute()
        {
            PatrolColor = Helper.RandomColor();
            Points = new();
        }
        public void addPoint(PointLatLng p)
        {
            if (Points.Count > int.Parse(ConfigurationManager.AppSettings.Get("Max_Location_History")))
            {
                Points.Dequeue();
            }
            Points.Enqueue(p);
        }
    }
}