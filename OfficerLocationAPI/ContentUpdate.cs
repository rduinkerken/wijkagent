namespace OfficerLocationAPI
{
    public enum UpdateType
    {
        Offense,
        NewOffense,
        Location
    }
    public class ContentUpdate
    {
        public ContentUpdate(int officerID, PointLatLng newPoint, string authKey)
        {
            Type = UpdateType.Location;
            AuthKey = authKey;
            ContentUpdateLocation = new()
            {
                OfficerID = officerID,
                NewPoint = newPoint 
            };
        }
        public ContentUpdate()
        {

        }
        public string AuthKey { get; set; }
        public UpdateType Type { get; set; }
        public ContentUpdateLocation ContentUpdateLocation { get; set; }
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