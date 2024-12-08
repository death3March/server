using MasterMemory;
using MessagePack;

namespace HackU_2024_server.DataBase;


[MemoryTable("Room")]
[MessagePackObject(true)]
public class Room
{
    [PrimaryKey] public string RoomName { get; set; } = string.Empty;
    public List<int> UserIDs { get; set; } = [];
    public Dictionary<int, int> UserOrder { get; set; } = new();
    public Dictionary<int, int> UserOtoshidama { get; set; } = new();
    public Dictionary<int, int> UserPosition { get; set; } = new();
    public Dictionary<int, bool> UserIsAnswered { get; set; } = new();
    public Dictionary<int, int> UserAnswer { get; set; } = new();
    public QuizStart? QuizData { get; set; } = null;
    public RoomState State { get; set; } = RoomState.Waiting;
    
    public enum RoomState
    {
        Waiting,
        Gaming,
        Result
    }
}