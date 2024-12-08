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
        ServerMessage? res = null;
        switch (data.TypeCase)
        {
            case ClientMessage.TypeOneofCase.None:
                break;
            case ClientMessage.TypeOneofCase.RoomJoinRequest:
                res = await OnRoomJoinRequest(client, data.RoomJoinRequest);
                break;
            case ClientMessage.TypeOneofCase.RoomLeaveRequest:
                res = await OnRoomLeaveRequest(client, data.RoomLeaveRequest);
                break;
            case ClientMessage.TypeOneofCase.GameStartRequest:
                res = await GameService.OnGameStartRequest(client, data.GameStartRequest);
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

    private static async UniTask<ServerMessage?> OnRoomJoinRequest(Client client, RoomJoinRequest req)
    {
        return await UniTask.Run<ServerMessage?>(() => { 
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
            return null;
        });
    }
    
    private static async UniTask<ServerMessage?> OnRoomLeaveRequest(Client client, RoomLeaveRequest req)
    {
        return await UniTask.Run<ServerMessage?>(() => { 
            var room = DataBaseManager.GetRoom(client.RoomName);
            if (room is null)
                return null;
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
            return null;
        });
    }
}