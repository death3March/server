using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using HackU_2024_server.DataBase;

namespace HackU_2024_server.Service;

public static class EventService
{
    public static async UniTask OnEventAsync(Client client, Data data)
    {
        var otherClients = DataBaseManager.GetClients(client.RoomName)
            .Where(c => c.GlobalUserId != client.GlobalUserId).ToArray();
        var isRelay = false;
        switch (data.TypeCase)
        {
            case Data.TypeOneofCase.RoomName:
                await RoomNameEventAsync(client, data);
                isRelay = true;
                break;
            case Data.TypeOneofCase.DisplayName:
                await DisplayNameEventAsync(client, data);
                isRelay = true;
                break;
            case Data.TypeOneofCase.Message:
                await MessageEventAsync(client, data);
                isRelay = true;
                break;
            case Data.TypeOneofCase.None:
                break;
            case Data.TypeOneofCase.Position:
                isRelay = true;
                break;
            case Data.TypeOneofCase.NewUserID:
                isRelay = true;
                break;
            case Data.TypeOneofCase.Event:
                EventAsync(client, data, otherClients).Forget();
                isRelay = false;
                break;
            case Data.TypeOneofCase.Otoshidama:
                isRelay = false;
                break;
            case Data.TypeOneofCase.Spaces:
                isRelay = false;
                break;
            case Data.TypeOneofCase.Roulette:
                break;
            case Data.TypeOneofCase.OtoshidamaTotal:
                isRelay = true;
                break;
            case Data.TypeOneofCase.Quiz:
                isRelay = false;
                // TODO: judge quiz answer and if correct, add otoshidama
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(data));
        }

        if (isRelay)
        {
            foreach (var c in otherClients)
            {
                c.SendAsync(data.ToByteArray()).Forget();
            }
        }
    }

    private static async UniTask RoomNameEventAsync(Client client, Data data)
    {
        await UniTask.Run(() =>
        {
            var clients = DataBaseManager.GetClients(data.RoomName);
            foreach (var c in clients)
            {
                var sendData = new Data
                {
                    DisplayName = c.DisplayName,
                    UserID = c.UserID
                };
                client.SendAsync(sendData.ToByteArray()).Forget();
            }
            client.RoomName = data.RoomName;
            DataBaseManager.UpdateClientData(client);
        });
    }
    
    private static async UniTask DisplayNameEventAsync(Client client, Data data)
    {
        await UniTask.Run(() =>
        {
            client.DisplayName = data.DisplayName;
            DataBaseManager.UpdateClientData(client);
        });
    }
    
    private static async UniTask MessageEventAsync(Client client, Data data)
    {
        await UniTask.Run(() =>
        {
            Console.WriteLine("Message: " + data.Message);
        });
    }

    private static async UniTask EventAsync(Client client, Data data, Client[] otherClients)
    {
        await UniTask.Run(async () =>
        {
            var eventType = data.Event;
            switch (eventType)
            {
                case Event.Roulette:
                    var positionResult = new Random().Next(1, 7);
                    var sendPositionResultData = new Data
                    {
                        UserID = data.UserID,
                        Roulette = new Roulette
                        {
                            Number = positionResult,
                            UserID = data.UserID
                        }
                    };
                    await client.SendAsync(sendPositionResultData.ToByteArray());
                    foreach (var c in otherClients)
                    {
                        c.SendAsync(sendPositionResultData.ToByteArray()).Forget();
                    }
                    
                    client.Position += positionResult;
                    DataBaseManager.UpdateClientData(client);
                    var positionData = new Data
                    {
                        Position = client.Position,
                        UserID = client.UserID
                    };
                    client.SendAsync(positionData.ToByteArray()).Forget();
                    foreach (var c in otherClients)
                    {
                        c.SendAsync(positionData.ToByteArray()).Forget();
                    }
                    break;
                case Event.Otoshidama:
                    var otoshidamaResult = new Random().Next(1, 11) * 1000;
                    var sendOtoshidamaResultData = new Data
                    {
                        UserID = data.UserID,
                        Otoshidama = new Otoshidama
                        {
                            Amount = otoshidamaResult,
                            UserID = data.UserID
                        }
                    };
                    await client.SendAsync(sendOtoshidamaResultData.ToByteArray());
                    foreach (var c in otherClients)
                    {
                        c.SendAsync(sendOtoshidamaResultData.ToByteArray()).Forget();
                    }
                    
                    client.Otoshidama += otoshidamaResult;
                    DataBaseManager.UpdateClientData(client);
                    var otoshidamaData = new Data
                    {
                        UserID = data.UserID,
                        OtoshidamaTotal = client.Otoshidama
                    };
                    client.SendAsync(otoshidamaData.ToByteArray()).Forget();
                    foreach (var c in otherClients)
                    {
                        c.SendAsync(otoshidamaData.ToByteArray()).Forget();
                    }
                    break;
                case Event.Furidashi:
                    foreach (var c in otherClients)
                    {
                        c.SendAsync(data.ToByteArray()).Forget();
                    }
                    client.Position = 0;
                    DataBaseManager.UpdateClientData(client);
                    var furidashiData = new Data
                    {
                        Position = 0,
                        UserID = client.UserID
                    };
                    client.SendAsync(furidashiData.ToByteArray()).Forget();
                    foreach (var c in otherClients)
                    {
                        c.SendAsync(furidashiData.ToByteArray()).Forget();
                    }
                    break;
                case Event.Join:
                    break;
                case Event.Leave:
                    break;
                case Event.Quiz:
                    var quiz = await MakeQuizAsync();
                    var quizData = new Data
                    {
                        Quiz = quiz
                    };
                    client.SendAsync(quizData.ToByteArray()).Forget();
                    foreach (var c in otherClients)
                    {
                        c.SendAsync(quizData.ToByteArray()).Forget();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "Invalid Event Type");
            }
        });
    }
    private static async UniTask<Quiz> MakeQuizAsync()
    {
        return await UniTask.Run(() =>
        {
            var quiz = new Quiz
            {
                Question = "問題",
                Choices = { "選択肢1", "選択肢2", "選択肢3", "選択肢4" },
                Answer = 1
            };
            return quiz;
        });
    }
}