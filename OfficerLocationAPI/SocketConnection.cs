namespace OfficerLocationAPI
{
    public static class SocketConnection {
        private static WebSocketSharp.WebSocket _webSocket { get; set; }
        public static void Init()
        {
            _webSocket = new WebSocketSharp.WebSocket("ws://url-goes-here:6969/Parser");

            // Set the WebSocket events.
            _webSocket.OnOpen += OnOpen;
            _webSocket.OnError += (sender, e) => Console.WriteLine("[WebSocket Error] {0}", e.Message);
            _webSocket.OnClose += (sender, e) => Console.WriteLine("[WebSocket Close ({0})] {1}", e.Code, e.Reason);

            _webSocket.Connect();
        }

        private static void OnOpen(object? sender, EventArgs e)
        {
            Console.WriteLine("Connected");
        }

        public static void SendMessage(ContentUpdate contentUpdate)
        {
            // convert object to json for easy transfer
            string msg = Newtonsoft.Json.JsonConvert.SerializeObject(contentUpdate);

            // send message to the server
            _webSocket.Send(msg);
        }
    }
}