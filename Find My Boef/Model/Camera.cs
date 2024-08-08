using Find_My_Boef.View;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Diagnostics;

namespace Find_My_Boef.Model
{
    public enum CameraStatus
    {
        Working,
        NotWorking
    }
    public class Camera : IIcon
    {
        public PointLatLng Location { get; set; }
        public CameraStatus Status { get; set; }
        public GMapMarker Marker { get; set; }
        public int CameraId { get; set; }
        public string Url { get; set; }

        public Camera(PointLatLng location, CameraStatus status, string url, int cameraId)
        {
            Location = location;
            Status = status;
            Url = url;
            CameraId = cameraId;
        }

        public void Draw()
        {
            Visualization.AddIconToMap(Location, (Status == CameraStatus.Working) ? Visualization.MarkerImage.CameraWorking : Visualization.MarkerImage.CameraNotWorking, this, CameraId);
        }

        public void ReDraw()
        {
            throw new System.NotImplementedException();
        }

        public void ReLocate(PointLatLng pointTo)
        {
            throw new System.NotImplementedException();
        }

        public void Remove()
        {
            Debug.WriteLine("Remove is not implemented yet.");
            // throw new System.NotImplementedException();
        }
    }
}
