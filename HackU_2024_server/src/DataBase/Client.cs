using System.Net;
using System.Net.WebSockets;
using Cysharp.Threading.Tasks;
using MasterMemory;
using MessagePack;

namespace HackU_2024_server.DataBase;

[MemoryTable("Client")]
[MessagePackObject(true)]
public class Client : IDisposable
{
    [PrimaryKey] public string GlobalUserId => UserID + RoomName;

    [SecondaryKey(0), NonUnique] public int UserID { get; set; } = 0;

    [SecondaryKey(1) , NonUnique]
    public string RoomName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
    
    public WebSocket? Socket { get; set; }

    public async UniTask InitializeAsync(HttpListenerContext context)
    {
        var taskHttpListenerContext = await context.AcceptWebSocketAsync(null);
        Socket = taskHttpListenerContext.WebSocket;
    }

    public async UniTask StartReceive(Func<Client, byte[], UniTask> receiveHandler, Func<Client, UniTask> closeHandler)
    {
        while (Socket is { State: WebSocketState.Connecting })
        {
            // receive data
            var buffer = new byte[1024];
            var result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                await closeHandler(this);
                break;
            }
            
            // handle received data
            await receiveHandler(this, buffer);
        }

        await closeHandler(this);
    }
    
    public async UniTask SendAsync(byte[] data)
    {
        if (Socket != null)
            await Socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true,
                CancellationToken.None);
    }

    public void Dispose()
    {
        Socket?.Dispose();
    }
}