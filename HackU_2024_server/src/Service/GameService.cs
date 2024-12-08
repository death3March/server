using Cysharp.Threading.Tasks;
using Google.Protobuf.Collections;
using HackU_2024_server.DataBase;

namespace HackU_2024_server.Service;

public static class GameService
{
    private static int Roulette(int min, int max)
    {
        var rnd = new Random();
        return rnd.Next(min, max + 1);
    }
    
    public static async UniTask<ServerMessage?> OnGameStartRequest(Client client, GameStartRequest req)
    {
        return await UniTask.Run<ServerMessage?>(() => { 
            var room = DataBaseManager.GetRoom(client.RoomName);
            if (room is null)
                return null;
            if (room.State != Room.RoomState.Waiting)
                return null;
            room.State = Room.RoomState.Gaming;
            var gameStartRes = new GameStart
            {
                Type = "GameStart",
                Data = new GameStart.Types.Data
                {
                    Map = GenerateMap()
                }
            };
            var res = new ServerMessage
            {
                GameStart = gameStartRes
            };
            return res;
        });
    }
    
    private static GameStart.Types.Data.Types.Map GenerateMap()
    {
        var squares = new RepeatedField<GameStart.Types.Data.Types.Map.Types.squareType>();
        for (var i = 0; i < 40; i++)
        {
            var num = new Random().Next(0, 4);
            squares.Add((GameStart.Types.Data.Types.Map.Types.squareType)num);
        }
        var map = new GameStart.Types.Data.Types.Map
        {
            Squares = {squares}
        };
        return map;
    }
    
    public static ServerMessage? TurnStart(Room room, int order)
    {
        var thisTurnUserID = room.UserOrder.FirstOrDefault(x => x.Value == order).Key;
        if(thisTurnUserID is 0) return null;
        var res = new ServerMessage
        {
            PlayerTurnStart = new PlayerTurnStart
            {
                Data = new PlayerTurnStart.Types.Data
                {
                    PlayerId = thisTurnUserID
                }
            }
        };
        return res;
    }
}