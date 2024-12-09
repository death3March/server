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
    
    public static async UniTask<ServerMessage[]?> OnGameStartRequest(Client client, GameStartRequest req)
    {
        return await UniTask.Run(() => { 
            var room = DataBaseManager.GetRoom(client.RoomName);
            if (room is null)
                return null;
            if (room.State != Room.RoomState.Waiting)
                return null;
            room.State = Room.RoomState.Gaming;
            
            var roomUsersNum = room.UserIDs.Count;

            var order = new int[roomUsersNum];
            for (var i = 0; i < roomUsersNum; i++)
            {
                order[i] = i;
            }
            //shuffle
            for (var i = 0; i < roomUsersNum; i++)
            {
                var j = Roulette(0, i);
                (order[i], order[j]) = (order[j], order[i]);
            }
            
            var userOrder = new Dictionary<string, int>();
            var userOtoshidama = new Dictionary<string, int>();
            var userPosition = new Dictionary<string, int>();
            var userIsAnswered = new Dictionary<string, bool>();
            var userAnswer = new Dictionary<string, int>();
            for (var i = 0; i < roomUsersNum; i++)
            {
                userOrder.Add(room.UserIDs[i], order[i]);
                userOtoshidama.Add(room.UserIDs[i], 0);
                userPosition.Add(room.UserIDs[i], 0);
                userIsAnswered.Add(room.UserIDs[i], false);
                userAnswer.Add(room.UserIDs[i], 0);
            }
            room.UserOrder = userOrder;
            room.UserOtoshidama = userOtoshidama;
            room.UserPosition = userPosition;
            room.UserIsAnswered = userIsAnswered;
            room.UserAnswer = userAnswer;
            DataBaseManager.UpdateRoomData(room);
            
            var gameStartRes = new ServerMessage
            {
                GameStart = new GameStart
                {
                    Type = "GameStart",
                    Data = new GameStart.Types.Data
                    {
                        Map = GenerateMap()
                    }
                }
            };
            var turnStartRes = TurnStart(room, 0);
            return turnStartRes is null ? null : ((ServerMessage[])[gameStartRes]).Concat(turnStartRes).ToArray();
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

    public static ServerMessage[]? NextTurn(Client client, TurnEndNotification req)
    {
        var thisTurnUserID = client.UserID;
        var room = DataBaseManager.GetRoom(client.RoomName);
        if (room is null) return null;
        var thisTurnOrder = room.UserOrder[thisTurnUserID];
        var nextTurnOrder = (thisTurnOrder + 1) % room.UserIDs.Count;
        var res = TurnStart(room, nextTurnOrder);
        return res;
    }
    
    public static ServerMessage[]? TurnStart(Room room, int order)
    {
        if (room.SugorokuMap is null)
        {
            return null;
        }
        var thisTurnUserID = room.UserOrder.FirstOrDefault(x => x.Value == order).Key;
        var dice = Roulette(1, 6);
        var thisTurnUserPosition = room.UserPosition[thisTurnUserID] + dice;
        room.UserPosition[thisTurnUserID] = thisTurnUserPosition;
        DataBaseManager.UpdateRoomData(room);
        
        if(thisTurnUserID is null) return null;
        var res = new List<ServerMessage>();
        var startRes = new ServerMessage
        {
            PlayerTurnStart = new PlayerTurnStart
            {
                Data = new PlayerTurnStart.Types.Data
                {
                    PlayerId = thisTurnUserID
                }
            }
        };
        res.Add(startRes);
        var moveRes = new ServerMessage
        {
            PlayerMovementDisplay = new PlayerMovementDisplay
            {
                Data = new PlayerMovementDisplay.Types.Data
                {
                    PlayerId = thisTurnUserID,
                    NewPosition = thisTurnUserPosition
                }
            }
        };
        res.Add(moveRes);
        if (room.SugorokuMap.Squares[thisTurnUserPosition] !=
            GameStart.Types.Data.Types.Map.Types.squareType.Otoshidama) return res.ToArray();
        var otoshidamaRes = new ServerMessage
        {
            OtoshidamaEvent = new OtoshidamaEvent
            {
                Data = new OtoshidamaEvent.Types.Data
                {
                    PlayerId = thisTurnUserID,
                    OtoshidamaAmount = Roulette(1, 10) * 1000,
                    //Message = "おとし玉イベント"
                }
            }
        };
        res.Add(otoshidamaRes);

        return res.ToArray();
    }
    
    public static ServerMessage[]? OnGameEndRequest()
    {
        return null;
    }
}