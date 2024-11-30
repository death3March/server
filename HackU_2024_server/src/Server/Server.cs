using System.Net.WebSockets;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cysharp.Threading.Tasks;
using HackU_2024_server.DataBase;

namespace HackU_2024_server.Server;

public static class Server
{
    private static bool _isRunning;
    private static readonly TcpListener TcpListener = new(IPAddress.Any, 8080);
    
    public static async UniTask Start()
    {
        Console.WriteLine("Server Start");
        
        _isRunning = true;
        
        TcpListener.Start();

        while (_isRunning)
        {
            var client = await TcpListener.AcceptTcpClientAsync();
            ReceiveHandler(client).Forget();
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
                case Data.TypeOneofCase.Event:
                default:
                    throw new ArgumentOutOfRangeException(nameof(client), receiveData.TypeCase.ToString());
            }
            if (c.Socket == null) continue;
            await c.SendAsync(data);
        }
    }

    private static async UniTask ReceiveHandler(TcpClient tcpClient)
    {
        var client = new Client();
        await client.InitializeAsync(tcpClient);
        client.StartReceive(ClientHandler, CloseHandler).Forget();
        DataBaseManager.AddClientData(client);
        Console.WriteLine("Client Added");
    }

    public static void Stop()
    {
        _isRunning = false;
    }
}