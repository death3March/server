using MasterMemory;
using Cysharp.Threading.Tasks;

namespace HackU_2024_server.DataBase;

public static class DataBaseManager{
    private static readonly DatabaseBuilder Builder = new();
    private static MemoryDatabase _db;
    
    static DataBaseManager(){
        _db = new MemoryDatabase(Builder.Build());
    }

    public static void AddClientData(in ClientData client){
        var builder = _db.ToImmutableBuilder();
        builder.Diff(new[] { client });
        _db = builder.Build();
    }

    public static RangeView<ClientData> GetClients(in string? roomName = null){
        return roomName is null ? _db.ClientDataTable.All : _db.ClientDataTable.FindByRoomName(roomName);
    }
}