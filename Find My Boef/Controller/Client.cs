using Find_My_Boef.DataContext;
using Find_My_Boef.Model;
using System;
using System.Configuration;
using System.Threading;
using System.Timers;
using System.Windows.Media;
using WebSocketSharp;

namespace Find_My_Boef.Controller
{
    public class Client
    {
        private static readonly int s_connectionWaitTime = int.Parse(ConfigurationManager.AppSettings.Get("Time_To_Reconnect"));
        private DateTime _lastConnectionAttemptDate { get; set; }
        private WebSocket _webSocket { get; set; }
        private static WebSocketStatusDataContext s_webSocketStatus = new();
        public static bool IsANotificationAlreadyVisible { get; set; }
        public static Notification? LoseWebsocketConnection { get; set; }

        public Client(string url)
        {
            _webSocket = new WebSocket(url);

            // Set the WebSocket events.
            _webSocket.OnOpen += OnOpen;
            _webSocket.OnMessage += OnReceive;
            _webSocket.OnError += (sender, e) => Console.WriteLine("[WebSocket Error] {0}", e.Message);
            _webSocket.OnClose += (sender, e) => Console.WriteLine("[WebSocket Close ({0})] {1}", e.Code, e.Reason);

            MainInstance.Timer += ConnectionChecker;
        }

        private void ConnectionChecker(object? sender, ElapsedEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                if (!IsConnected())
                {
                    s_webSocketStatus.StatusColor = new SolidColorBrush(Colors.Salmon);
                    s_webSocketStatus.StatusMessage = "Niet verbonden";
                    if (!IsANotificationAlreadyVisible)
                    {
                        LoseWebsocketConnection = NotificationDataContext.CreateNotification("Websocket", "De verbinding met de websocket is verbroken.");
                        LoseWebsocketConnection.Topmost = true;
                        IsANotificationAlreadyVisible = true;
                    }

                    RetryConnection();
                }
                else
                {
                    s_webSocketStatus.StatusColor = new SolidColorBrush(Colors.Green);
                    s_webSocketStatus.StatusMessage = "Verbonden";
                    if (IsANotificationAlreadyVisible)
                    {
                        NotificationDataContext.HideNotification(LoseWebsocketConnection);
                        IsANotificationAlreadyVisible = false;
                    }
                }
            });
        }

        // Function to retry connecting every {s_connectionWaitTime} seconds
        private void RetryConnection()
        {
            int diffInSeconds = (int)(DateTime.Now - _lastConnectionAttemptDate).TotalSeconds;

            if (s_connectionWaitTime < diffInSeconds)
            {
                SocketConnect();
            }
        }

        public void SocketConnect()
        {
            _lastConnectionAttemptDate = DateTime.Now;

            // Connect synchronously on new thread to prevent slowdowns
            new Thread(() =>
            {
                _webSocket.Connect();
            }).Start();
        }

        public void SendMessage(ContentUpdate contentUpdate)
        {
            // Convert object to json for easy transfer
            string msg = Newtonsoft.Json.JsonConvert.SerializeObject(contentUpdate);

            // Send message to the server
            _webSocket.Send(msg);
        }

        // Message received from server
        private void OnReceive(object? sender, MessageEventArgs e)
        {
            if (e.IsPing)
            {
                return;
            }

            string json = e.Data;

            // Use newtonsoft to convert json back to object
            ContentUpdate contentUpdate = Newtonsoft.Json.JsonConvert.DeserializeObject<ContentUpdate>(json);

            // Use object to update content
            if (contentUpdate != null)
            {
                UpdateContent(contentUpdate);
            }
        }

        // Connected to the server
        private void OnOpen(object? sender, EventArgs e)
        {
            Console.WriteLine("Connected");
        }

        // Called whenever websocket sends a content update
        private void UpdateContent(ContentUpdate contentUpdate)
        {
            switch (contentUpdate.Type)
            {
                case UpdateType.Offense:
                    ContentUpdateOffense cuo = contentUpdate.ContentUpdateOffense;

                    OffenseDataContext.CurrentDescription = cuo.NewDescription;
                    OffenseDataContext.CurrentStatus = cuo.NewStatus.ToString();
                    OffenseDataContext.CurrentOffenseType = cuo.NewOffenseType.ToString();

                    MainInstance.GetOffensesFromID(cuo.OffenseID).Description = cuo.NewDescription;
                    MainInstance.GetOffensesFromID(cuo.OffenseID).Status = cuo.NewStatus;
                    MainInstance.GetOffensesFromID(cuo.OffenseID).Type = cuo.NewOffenseType;

                    MainInstance.GetOffensesFromID(cuo.OffenseID).ReDraw();
                    break;
                case UpdateType.Location:
                    ContentUpdateLocation cul = contentUpdate.ContentUpdateLocation;
                    MainInstance.GetOfficerFromId(cul.OfficerID)?.ReLocate(cul.NewPoint);
                    break;
                case UpdateType.NewOffense:
                    ContentUpdateNewOffense cuno = contentUpdate.ContentUpdateNewOffense;
                    Offense o = new(cuno.OffenseID, cuno.OffenseType, DateTime.Now, cuno.Description, cuno.Status, cuno.Point);
                    MainInstance.AddOffense(o);
                    o.Draw();
                    break;
            }
        }

        // Function to check the connection
        public bool IsConnected()
        {
            return _webSocket.IsAlive;
        }

        // Setter for datacontext for xaml connection
        public static void SetWebSocketStatus(WebSocketStatusDataContext dataContext)
        {
            s_webSocketStatus = dataContext;
        }
    }
}
