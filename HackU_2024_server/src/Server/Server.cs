using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Cysharp.Threading.Tasks;
using HackU_2024_server.DataBase;
using HackU_2024_server.Service;

namespace HackU_2024_server.Server;

public static class Server
{
    private static bool _isRunning;
    private static readonly TcpListener TcpListener = new(IPAddress.Any, 8080);
    private static readonly TcpListener HealthCheckTcpListener = new(IPAddress.Any, 8081);

    public static async UniTask Start()
    {
        Console.WriteLine("Server Start");

        _isRunning = true;

        TcpListener.Start();
        HealthCheckTcpListener.Start();

        HealthCheckHandler().Forget();

        while (_isRunning)
        {
            var client = await TcpListener.AcceptTcpClientAsync();
            ReceiveHandler(client).Forget();
        }
    }

    private static async UniTask CloseHandler(Client client)
    {
        await UniTask.Run(() =>
        {
            if(client.RoomName == string.Empty)
            {
                DataBaseManager.RemoveClientData(client);
            }
            client.Socket?.Dispose();
            client.Socket = null;
            DataBaseManager.UpdateClientData(client);
        });
    }
    private static async UniTask HealthCheckHandler()
    {
        while (_isRunning)
        {
            var healthCheckClient = await HealthCheckTcpListener.AcceptTcpClientAsync();
            await using var stream = healthCheckClient.GetStream();
            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer);
            var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if (request.StartsWith("GET"))
            {
                var response = "HTTP/1.1 200 OK\r\nContent-Length: 2\r\n\r\nOK"u8.ToArray();
                await stream.WriteAsync(response);
            }
            else
            {
                var response = "HTTP/1.1 404 Not Found\r\nContent-Length: 0\r\n\r\n"u8.ToArray();
                await stream.WriteAsync(response);
            }

            healthCheckClient.Close();
        }
    }

    private static async UniTask ClientHandler(Client client, byte[] data)
    {
        var recvData = ClientMessage.Parser.ParseFrom(data);
        await EventService.OnReceiveAsync(client, recvData);
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