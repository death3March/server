using System.Net.WebSockets;
using System.Net;
using Cysharp.Threading.Tasks;
using HackU_2024_server.DataBase;

namespace HackU_2024_server.Server;

public static class Server
{
    private static bool _isRunning;
    private static readonly HttpListener HttpListener = new();
    
    public static async UniTask Start()
    {
        Console.WriteLine("Server Start");
        
        _isRunning = true;
        
        HttpListener.Prefixes.Add("http://127.0.0.1:8080/");
        HttpListener.Start();

        while (_isRunning)
        {
            var context = await HttpListener.GetContextAsync();
            Console.WriteLine(context.Request.ToString());
            Console.WriteLine(context.Request.IsWebSocketRequest);
            if (!context.Request.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;	// bad request
                context.Response.Close();
                Console.WriteLine("Not WebSocket Request");
                continue;
            }
            ReceiveHandler(context).Forget();
        }
    }

    private static async UniTask CloseHandler(Client client)
    {
        var clients = DataBaseManager.GetClients(client.RoomName).Where(c => c.GlobalUserId != client.GlobalUserId);
        foreach (var c in clients)
        {
            if (c.Socket != null)
                await c.Socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
        }
        DataBaseManager.RemoveClientData(client);
    }

    private static async UniTask ClientHandler(Client client, byte[] data)
    {
        var clients = DataBaseManager.GetClients(client.RoomName).ToArray();
        foreach (var c in clients)
        {
            var receiveData = Data.Parser.ParseFrom(data);
            switch (receiveData.TypeCase)
            {
                case Data.TypeOneofCase.RoomName:
                    c.RoomName = receiveData.RoomName;
                    Console.WriteLine("RoomName: " + c.RoomName);
                    break;
                case Data.TypeOneofCase.DisplayName:
                    c.DisplayName = receiveData.DisplayName;
                    Console.WriteLine("DisplayName: " + c.DisplayName);
                    break;
                case Data.TypeOneofCase.Message:
                    Console.WriteLine("Message: " + receiveData.Message);
                    break;
                case Data.TypeOneofCase.Point:
                    Console.WriteLine("Point: " + receiveData.Point);
                    break;
                case Data.TypeOneofCase.NewUserID:
                    Console.WriteLine("NewUserID: " + receiveData.NewUserID);
                    break;
                case Data.TypeOneofCase.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(client), receiveData.TypeCase.ToString());
            }
            if (c.Socket == null) continue;
            await c.SendAsync(data);
        }
    }

    private static async UniTask ReceiveHandler(HttpListenerContext context)
    {
        var client = new Client();
        await client.InitializeAsync(context);
        client.StartReceive(ClientHandler, CloseHandler).Forget();
        DataBaseManager.AddClientData(client);
        Console.WriteLine("Client Added");
    }

    public static void Stop()
    {
        _isRunning = false;
        HttpListener.Stop();
        HttpListener.Close();
    }
}