using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace Find_My_Boef.Model
{
    public interface IIcon
    {
        GMapMarker Marker { get; set; }
        void Draw();
        void Remove();
        void ReDraw();
        void ReLocate(PointLatLng pointTo);
    }
}
