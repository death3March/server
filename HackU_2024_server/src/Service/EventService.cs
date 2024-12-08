using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using HackU_2024_server.DataBase;

namespace HackU_2024_server.Service;

public static class EventService
{
    public static async UniTask OnReceiveAsync(Client client, ClientMessage data)
    {
        Console.WriteLine(nameof(data.TypeCase));
        switch (data.TypeCase)
        {
            case ClientMessage.TypeOneofCase.None:
                break;
            case ClientMessage.TypeOneofCase.RoomJoinRequest:
                await OnRoomJoinRequest(client, data.RoomJoinRequest);
                break;
            case ClientMessage.TypeOneofCase.RoomLeaveRequest:
                await OnRoomLeaveRequest(client, data.RoomLeaveRequest);
                break;
            case ClientMessage.TypeOneofCase.GameStartRequest:
                await OnGameStartRequest(client, data.GameStartRequest);
                break;
            case ClientMessage.TypeOneofCase.GameEndRequest:
                break;
            case ClientMessage.TypeOneofCase.QuizAnswer:
                break;
            case ClientMessage.TypeOneofCase.TurnEndNotification:
                break;
            case ClientMessage.TypeOneofCase.OtherMessage:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(data));
        }
    }

    private static async UniTask OnRoomJoinRequest(Client client, RoomJoinRequest req)
    {
        await UniTask.Run(() => { 
            var roomName = req.Data.RoomCode;
            var room = DataBaseManager.GetRoom(roomName);
            if (room is null)
            {
                room = new Room
                {
                    RoomName = roomName,
                    UserIDs = {client.UserID},
                    UserOrder = {[client.UserID] = 0},
                    UserOtoshidama = {[client.UserID] = 0},
                    UserPosition = {[client.UserID] = 0},
                    UserIsAnswered = {[client.UserID] = false},
                    UserAnswer = {[client.UserID] = 0}
                };
                DataBaseManager.AddRoomData(room);
            }
            else
            {
                room.UserIDs.Add(client.UserID);
                room.UserOrder.Add(client.UserID, room.UserIDs.Count - 1);
                room.UserOtoshidama.Add(client.UserID, 0);
                room.UserPosition.Add(client.UserID, 0);
                room.UserIsAnswered.Add(client.UserID, false);
                room.UserAnswer.Add(client.UserID, 0);
                DataBaseManager.UpdateRoomData(room);
            }
            
            client.RoomName = roomName;
            DataBaseManager.UpdateClientData(client);
            Console.WriteLine("Room Joined");
        });
    }
    
    private static async UniTask OnRoomLeaveRequest(Client client, RoomLeaveRequest req)
    {
        await UniTask.Run(() => { 
            var room = DataBaseManager.GetRoom(client.RoomName);
            if (room is null)
                return;
            room.UserIDs.Remove(client.UserID);
            room.UserOrder.Remove(client.UserID);
            room.UserOtoshidama.Remove(client.UserID);
            room.UserPosition.Remove(client.UserID);
            room.UserIsAnswered.Remove(client.UserID);
            room.UserAnswer.Remove(client.UserID);
            DataBaseManager.UpdateRoomData(room);
            
            client.RoomName = string.Empty;
            DataBaseManager.UpdateClientData(client);
            Console.WriteLine("Room Left");
        });
    }
    
    private static async UniTask OnGameStartRequest(Client client, GameStartRequest req)
    {
        await UniTask.Run(() => { 
            var room = DataBaseManager.GetRoom(client.RoomName);
            if (room is null)
                return;
            if (room.State != Room.RoomState.Waiting)
                return;
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
            var clients = DataBaseManager.GetClients(client.RoomName);
            foreach (var c in clients)
            {
                c.SendAsync(res.ToByteArray()).Forget();
            }
            DataBaseManager.UpdateRoomData(room);
            Console.WriteLine("Game Started");
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
}