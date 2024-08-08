using Find_My_Boef.Controller;
using Find_My_Boef.Model;
using System.Data.SqlClient;
using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Configuration;
using System.Net.Sockets;

namespace SocketServer
{
    internal class Parser : WebSocketBehavior
    {
        // Received message
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("{0} > {1}", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), e.Data);
            string json = e.Data;

            // Use newtonsoft to convert json back to object
            ContentUpdate contentUpdate = Newtonsoft.Json.JsonConvert.DeserializeObject<ContentUpdate>(json);

            // Use object to update content
            if (contentUpdate != null)
            {
                string query = "SELECT * FROM SocketServerAuth WHERE AuthKey=@AuthKey";
                SqlCommand command = new(query, Database.Connection);
                SqlParameter authKeyParam = new("@AuthKey", System.Data.SqlDbType.VarChar, 255);
                authKeyParam.Value = contentUpdate.AuthKey;
                command.Parameters.Add(authKeyParam);
                command.Prepare();
                SqlDataReader reader = command.ExecuteReader();
                if (!reader.Read())
                {
                    return;
                }
                if (contentUpdate.Type == UpdateType.Location)
                {
                    query = "UPDATE Locatie\r\nSET HuidigeLocatie=@Locatie, LaatsteUpdate=GETDATE()\r\nWHERE Werknemersnummer=@Werknemersnummer";
                    command = new(query, Database.Connection);
                    ContentUpdateLocation cul = contentUpdate.ContentUpdateLocation;
                    SqlParameter employeeNumParam = new("@Werknemersnummer", System.Data.SqlDbType.Int);
                    employeeNumParam.Value = cul.OfficerID;
                    command.Parameters.Add(employeeNumParam);
                    SqlParameter locationParam = new("@Locatie", System.Data.SqlDbType.VarChar, 255);
                    locationParam.Value = (cul.NewPoint.Lat + ";" + cul.NewPoint.Lng).Replace(',', '.');
                    command.Parameters.Add(locationParam);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
            }

            Sessions.Broadcast(e.Data);
        }

        // Client connected
        protected override void OnOpen()
        {
            Console.WriteLine("Client connected");
        }

        // Client disconnected
        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("Client disconnected");
        }
    }
}
