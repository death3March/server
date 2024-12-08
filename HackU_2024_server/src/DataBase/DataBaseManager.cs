using MasterMemory;
using Cysharp.Threading.Tasks;

namespace HackU_2024_server.DataBase;

public static class DataBaseManager{
    private static readonly DatabaseBuilder Builder = new();
    private static MemoryDatabase _db;
    
    static DataBaseManager(){
        _db = new MemoryDatabase(Builder.Build());
    }

    public static void AddClientData(in Client client){
        var builder = _db.ToImmutableBuilder();
        builder.Diff([client]);
        _db = builder.Build();
    }
    
    public static void UpdateClientData(in Client client){
        var builder = _db.ToImmutableBuilder();
        builder.Diff([client]);
        _db = builder.Build();
    }

    public static RangeView<Client> GetClients(in string? roomName = null){
        return roomName is null ? _db.ClientTable.All : _db.ClientTable.FindByRoomName(roomName);
    }
    
    public static void RemoveClientData(in Client client){
        var builder = _db.ToImmutableBuilder();
        builder.RemoveClient([
            client.GlobalUserId
        ]);
        _db = builder.Build();
    }
    
    public static void AddRoomData(in Room room){
        var builder = _db.ToImmutableBuilder();
        builder.Diff([room]);
        _db = builder.Build();
    }
    
    public static void UpdateRoomData(in Room room){
        var builder = _db.ToImmutableBuilder();
        builder.Diff([room]);
        _db = builder.Build();
    }
    
    public static Room? GetRoom(in string roomName)
    {
        try
        {
            return _db.RoomTable.FindByRoomName(roomName);
        } catch {
            return null;
        }
    }
    
    public static void RemoveRoomData(in Room room){
        var builder = _db.ToImmutableBuilder();
        builder.RemoveRoom([
            room.RoomName
        ]);
        _db = builder.Build();
    }
}