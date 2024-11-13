using MasterMemory;
using Cysharp.Threading.Tasks;

namespace HackU_2024_server.DataBase;

public static class DataBaseManager{
    private static DataBaseBuilder _builder = new();
    private static MemoryDatabase _db;
    
    public DataBaseManager(){
        _db = new MemoryDatabase(_builder.Build());
    }

    public static void AddClientData(in ClientData client){
        var builder = _db.ToImmutableBuilder();
        builder.Diff(new[] { client });
        _db = builder.Build();
    }

    public static Range<ClientData> GetClients(in string? roomName = null){
        return roomName is null ? _db.ClientDataTable.All : _db.ClientDataTable.FindByRoomName(roomName.Value);
    }
}