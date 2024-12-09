using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using MasterMemory;
using MessagePack;

namespace HackU_2024_server.DataBase;

[MemoryTable("Client")]
[MessagePackObject(true)]
public partial class Client : IDisposable
{
    [PrimaryKey] public string GlobalUserId => UserID + RoomName;

    [SecondaryKey(0)] public string UserID { get; set; } = string.Empty;

    [SecondaryKey(1)] [NonUnique] public string RoomName { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    public WebSocket? Socket { get; private set; }

    private TcpClient? TcpClient { get; set; }

    public void Dispose()
    {
        Socket?.Dispose();
    }

    public async UniTask InitializeAsync(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
        var stream = TcpClient.GetStream();
        var buffer = new byte[1024];
        var bytesRead = await stream.ReadAsync(buffer);
        var request = Encoding.UTF8.GetString(buffer, 0, bytesRead); // WebSocketハンドシェイクの処理
        if (request.Contains("Upgrade: websocket"))
        {
            var response = "HTTP/1.1 101 Switching Protocols\r\n" + "Connection: Upgrade\r\n" +
                           "Upgrade: websocket\r\n" + "Sec-WebSocket-Accept: " + Convert.ToBase64String(SHA1.Create()
                               .ComputeHash(Encoding.UTF8.GetBytes(
                                   MyRegex().Match(request)
                                       .Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))) + "\r\n\r\n";
            var responseBytes = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseBytes);
            UserID = Guid.NewGuid().ToString();
        }
    }

    public async UniTask StartReceive(Func<Client, byte[], UniTask> receiveHandler, Func<Client, UniTask> closeHandler)
    {
        if (TcpClient == null)
            return;
        Socket = WebSocket.CreateFromStream(TcpClient.GetStream(), true, null, TimeSpan.FromMinutes(2));
        Console.WriteLine("WebSocket Connected");
        while (Socket.State == WebSocketState.Open)
        {
            Console.WriteLine("Waiting for receiving");
            // receive data
            var buffer = new byte[8192];
            var result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                await closeHandler(this);
                break;
            }

            var size = result.Count;
            var byteArray = new byte[size];
            Array.Copy(buffer, byteArray, size);
            Console.WriteLine("handle received data");
            // handle received data
            await receiveHandler(this, byteArray);
        }

        await closeHandler(this);
    }

    public async UniTask SendAsync(byte[] data)
    {
        if (Socket != null)
            await Socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true,
                CancellationToken.None);
    }


    [GeneratedRegex("Sec-WebSocket-Key: (.*)")]
    private static partial Regex MyRegex();
}