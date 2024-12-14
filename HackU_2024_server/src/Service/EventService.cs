using Cysharp.Threading.Tasks;
using Google.Protobuf;
using HackU_2024_server.DataBase;

namespace HackU_2024_server.Service;

public static class EventService
{
    private const int SendDelay = 500;
    public static async UniTask OnReceiveAsync(Client client, ClientMessage data)
    {
        Console.WriteLine(data.TypeCase.ToString());

        ServerMessage[]? res = null;
        switch (data.TypeCase)
        {
            case ClientMessage.TypeOneofCase.None:
                break;
            case ClientMessage.TypeOneofCase.RoomJoinRequest:
                res = await OnRoomJoinRequest(client, data.RoomJoinRequest);
                await Task.Delay(1000);
                Console.WriteLine(res is null);
                break;
            case ClientMessage.TypeOneofCase.RoomLeaveRequest:
                res = await OnRoomLeaveRequest(client, data.RoomLeaveRequest);
                break;
            case ClientMessage.TypeOneofCase.GameStartRequest:
                res = await GameService.OnGameStartRequest(client, data.GameStartRequest);
                break;
            case ClientMessage.TypeOneofCase.GameEndRequest:
                res = GameService.OnGameEndRequest(client, data.GameEndRequest);
                break;
            case ClientMessage.TypeOneofCase.QuizAnswer:
                res = await QuizService.OnQuizAnswerAsync(client, data.QuizAnswer);
                var correctUsers = QuizService.CheckCorrect(client.RoomName);
                if (correctUsers is not null)
                {
                    var resultRes = QuizService.QuizResult(client.RoomName, correctUsers);
                    if (resultRes is not null)
                    {
                        res = res is null ? resultRes : res.Concat(resultRes).ToArray();
                    }
                }
                break;
            case ClientMessage.TypeOneofCase.TurnEndNotification:
                res = GameService.NextTurn(client, data.TurnEndNotification);
                break;
            case ClientMessage.TypeOneofCase.OtherMessage:
                if (data.OtherMessage.Data.Message == "Return")
                {
                    res = ReturnPlayer(client, data.OtherMessage);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(data));
        }

        if (res is not null)
        {
            var clients = DataBaseManager.GetClients(client.RoomName).Where(c => c.Socket != null).ToArray();
            Console.WriteLine("Send Response");
            foreach (var r in res)
            {
                foreach (var c in clients)
                {
                    Console.WriteLine("Send Response to " + c.UserID);
                    c.SendAsync(r.ToByteArray()).Forget();
                }

                await Task.Delay(SendDelay);
            }
        }
    }

    private static async UniTask<ServerMessage[]?> OnRoomJoinRequest(Client client, RoomJoinRequest req)
    {
        return await UniTask.Run<ServerMessage[]?>(() =>
        {
            var roomName = req.Data.RoomCode;
            var room = DataBaseManager.GetRoom(roomName);
            if (room is null)
            {
                CreateRoom(client, roomName);
            }
            else
            {
                room.UserIDs.Add(client.UserID);
                room.UserOrder.Add(client.UserID, room.UserIDs.Count - 1);
                room.UserOtoshidama.Add(client.UserID, 0);
                room.UserPosition.Add(client.UserID, 0);
                room.UserIsAnswered.Add(client.UserID, false);
                room.UserAnswer.Add(client.UserID, string.Empty);
                room.UserIsAnsweredOrder.Add(client.UserID, null);
                DataBaseManager.UpdateRoomData(room);
            }

            client.RoomName = roomName;
            client.Nickname = req.Data.Nickname;
            DataBaseManager.UpdateClientData(client);
            Console.WriteLine("Room Joined");
            var res = new ServerMessage
            {
                RoomJoinResponse = new RoomJoinResponse
                {
                    Data = new RoomJoinResponse.Types.Data
                    {
                        RoomCode = roomName,
                        PlayerId = client.UserID,
                        Nickname = client.Nickname
                    }
                }
            };
            client.SendAsync(res.ToByteArray()).Forget();
            Console.WriteLine("player id : " + ServerMessage.Parser.ParseFrom(res.ToByteArray()).RoomJoinResponse.Data.PlayerId);

            UniTask.Run(() =>
            {
                var alreadyJoinedClients = DataBaseManager.GetClients(roomName)
                    .Where(c => c.GlobalUserId != client.GlobalUserId);
                foreach (var c in alreadyJoinedClients)
                {
                    var res2 = new ServerMessage
                    {
                        RoomJoinResponse = new RoomJoinResponse
                        {
                            Data = new RoomJoinResponse.Types.Data
                            {
                                RoomCode = roomName,
                                PlayerId = c.UserID,
                                Nickname = c.Nickname
                            }
                        }
                    };
                    client.SendAsync(res2.ToByteArray()).Forget();
                }
            });
            
            Console.WriteLine(res.RoomJoinResponse.Data.PlayerId);

            return [res];
        });
    }

    private static void CreateRoom(Client client, string roomName)
    {
        var room = new Room
        {
            State = Room.RoomState.Waiting,
            RoomName = roomName,
            UserIDs = { client.UserID },
            UserOrder = { [client.UserID] = 0 },
            UserOtoshidama = { [client.UserID] = 0 },
            UserPosition = { [client.UserID] = 0 },
            UserIsAnswered = { [client.UserID] = false },
            UserAnswer = { [client.UserID] = string.Empty },
            UserIsAnsweredOrder = { [client.UserID ] = null}
        };
        Console.WriteLine("Room Created roomName : " + roomName);
        DataBaseManager.AddRoomData(room);
    }

    // ReSharper disable once UnusedParameter.Local
    private static async UniTask<ServerMessage[]?> OnRoomLeaveRequest(Client client, RoomLeaveRequest req)
    {
        return await UniTask.Run<ServerMessage[]?>(() =>
        {
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

    private static ServerMessage[]? ReturnPlayer(Client client, OtherMessage msg)
    {
        var beforeClient = DataBaseManager.GetClients(msg.Data.PlayerId);
        if (beforeClient.Count is not 1)
            return null;
        client.UserID = msg.Data.PlayerId;
        client.Nickname = beforeClient[0].Nickname;
        client.RoomName = beforeClient[0].RoomName;
        DataBaseManager.RemoveClientData(beforeClient[0]);
        DataBaseManager.UpdateClientData(client);
        var room = DataBaseManager.GetRoom(client.RoomName);
        if (room is null)
            return null;
        var roomState = new ServerMessage
        {
            RoomState = new RoomState
            {
                Data = new RoomState.Types.Data
                {
                    RoomCode = room.RoomName,
                    UserIDs = { room.UserIDs },
                    UserOrder = { room.UserOrder },
                    UserOtoshidama = { room.UserOtoshidama },
                    UserPosition = { room.UserPosition },
                    UserIsAnswered = { room.UserIsAnswered },
                    UserAnswer = { room.UserAnswer },
                    QuizData = room.QuizData,
                    State = (RoomState.Types.RoomState)room.State,
                    SugorokuMap = room.SugorokuMap,
                    CurrentTurnPlayerId = room.CurrentTurnPlayerId
                }
            }
        };
        client.SendAsync(roomState.ToByteArray()).Forget();
        var res = new ServerMessage
        {
            ReturnPlayerId = client.UserID
        };
        return [res];
    }
}