using System.Net;
using System.Net.WebSockets;
using Cysharp.Threading.Tasks;
using MasterMemory;
using MessagePack;

namespace HackU_2024_server.DataBase;

[MemoryTable("ClientData")]
[MessagePackObject(true)]
public class ClientData : IDisposable
{
    [PrimaryKey] public string GlobalUserId => UserID + RoomName;

    [SecondaryKey(0) , NonUnique]
    public int UserID { get; set; }

    [SecondaryKey(1) , NonUnique]
    public string RoomName { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    
    public WebSocket? Socket { get; set; }

    public async UniTask InitializeAsync(HttpListenerContext context)
    {
        var taskHttpListenerContext = await context.AcceptWebSocketAsync(null);
        Socket = taskHttpListenerContext.WebSocket;
    }

    public void Dispose()
    {
        Socket?.Dispose();
    }
}