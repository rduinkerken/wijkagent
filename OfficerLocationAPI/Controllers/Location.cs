using Microsoft.AspNetCore.Mvc;

namespace OfficerLocationAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LocationController : ControllerBase
    {
        //POST: /api/web/[value]
        [Route("{id}/{location}")]
        public void WebPost(string id, string location)
        {
            Console.WriteLine(id);
            Console.WriteLine(location);

            string[] split = location.Split(';');

            ContentUpdate cu = new(int.Parse(id),
                new PointLatLng(Double.Parse(split[1].Replace('.', ',')), Double.Parse(split[0].Replace('.', ','))),
                "JFUvPoFV7IItbHeycHqdOOgcg9n1UKvz");

            SocketConnection.SendMessage(cu);
        }
    }
}