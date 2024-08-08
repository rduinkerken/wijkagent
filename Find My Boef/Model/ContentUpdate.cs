using GMap.NET;

namespace Find_My_Boef.Model
{
    public enum UpdateType
    {
        Offense,
        NewOffense,
        Location
    }
    public class ContentUpdate
    {
        public ContentUpdate(UpdateType type)
        {
            Type = type;
        }
        public ContentUpdate()
        {

        }
        public UpdateType Type { get; set; }
        public string AuthKey { get; set; }
        public ContentUpdateOffense ContentUpdateOffense { get; set; }
        public ContentUpdateLocation ContentUpdateLocation { get; set; }
        public ContentUpdateNewOffense ContentUpdateNewOffense { get; set; }
        public ContentUpdateDeleteOffense ContentUpdateDeleteOffense { get; set; }
    }
    public class ContentUpdateOffense
    {
        public int OffenseID { get; set; }
        public Status NewStatus { get; set; }
        public OffenseType NewOffenseType { get; set; }
        public string? NewDescription { get; set; }
        public ContentUpdateOffense()
        {

        }
    }
    public class ContentUpdateNewOffense
    {
        public int OffenseID { get; set; }
        public PointLatLng Point { get; set; }
        public Status Status { get; set; }
        public OffenseType OffenseType { get; set; }
        public string Description { get; set; }
        public ContentUpdateNewOffense()
        {

        }
    }
    public class ContentUpdateDeleteOffense
    {
        public int OffenseID { get; set; }
        public ContentUpdateDeleteOffense()
        {

        }
    }
    public class ContentUpdateLocation
    {
        public int OfficerID { get; set; }
        public PointLatLng NewPoint { get; set; }
        public ContentUpdateLocation()
        {

        }
    }
}
