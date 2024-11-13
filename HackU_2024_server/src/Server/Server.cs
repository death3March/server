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
        _isRunning = true;
        
        HttpListener.Prefixes.Add("http://127.0.0.1:8080/");
        HttpListener.Start();

        while (_isRunning)
        {
            var context = await HttpListener.GetContextAsync();
            if (!context.Request.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;	// bad request
                context.Response.Close();
                continue;
            }
            ReceiveHandler(context).Forget();
        }
    }

    private static async UniTask CloseHandler(Client client)
    {
        var clients = DataBaseManager.GetClients(client.RoomName).ToArray();
        foreach (var c in clients)
        {
            if (c.Socket != null)
                await c.Socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
        }
    }

    private static async UniTask ClientHandler(Client client, byte[] data)
    {
        var clients = DataBaseManager.GetClients(client.RoomName).ToArray();
        foreach (var c in clients)
        {
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
    }

    public static void Stop()
    {
        _isRunning = false;
        HttpListener.Stop();
        HttpListener.Close();
    }
}