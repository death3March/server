﻿using Cysharp.Threading.Tasks;
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
        return await UniTask.Run(() =>
        {
            var room = DataBaseManager.GetRoom(client.RoomName);
            if (room is null)
            {
                Console.WriteLine("room is null");
                return null;
            }
            if (room.State != Room.RoomState.Waiting)
            {
                Console.WriteLine("room state is not waiting");
                return null;
            }
            Console.WriteLine("GameStartRequest");
            room.State = Room.RoomState.Gaming;

            var roomUsersNum = room.UserIDs.Count;

            var order = new int[roomUsersNum];
            for (var i = 0; i < roomUsersNum; i++) order[i] = i;
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
            var userAnswer = new Dictionary<string, string>();
            var userIsAnsweredOrder = new Dictionary<string, int?>();
            for (var i = 0; i < roomUsersNum; i++)
            {
                userOrder.Add(room.UserIDs[i], order[i]);
                userOtoshidama.Add(room.UserIDs[i], 0);
                userPosition.Add(room.UserIDs[i], 0);
                userIsAnswered.Add(room.UserIDs[i], false);
                userAnswer.Add(room.UserIDs[i], string.Empty);
                userIsAnsweredOrder.Add(room.UserIDs[i], null);
            }

            room.UserOrder = userOrder;
            room.UserOtoshidama = userOtoshidama;
            room.UserPosition = userPosition;
            room.UserIsAnswered = userIsAnswered;
            room.UserAnswer = userAnswer;
            room.UserIsAnsweredOrder = userIsAnsweredOrder;
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
            room.SugorokuMap = gameStartRes.GameStart.Data.Map;
            DataBaseManager.UpdateRoomData(room);
            Console.WriteLine("create GameStart res");
            var turnStartRes = TurnStart(room, 0);
            Console.WriteLine("create TurnStart res");
            return  turnStartRes is null ? null : ((ServerMessage[]) [gameStartRes]).Concat(turnStartRes).ToArray();
        });
    }

    private static GameStart.Types.Data.Types.Map GenerateMap()
    {
        var squares = new RepeatedField<GameStart.Types.Data.Types.Map.Types.squareType>();
        
        for (var i = 0; i < 100; i++)
        {
            var num = new Random().Next(0, 10);
            switch (num)
            {
                case < 6:
                    squares.Add(GameStart.Types.Data.Types.Map.Types.squareType.Quiz);
                    break;
                case < 8:
                    squares.Add(GameStart.Types.Data.Types.Map.Types.squareType.Otoshidama);
                    break;
                case < 9:
                    squares.Add(GameStart.Types.Data.Types.Map.Types.squareType.Furidashi);
                    break;
                default:
                    squares.Add(GameStart.Types.Data.Types.Map.Types.squareType.Normal);
                    break;
            }
        }
        
        squares[0] = GameStart.Types.Data.Types.Map.Types.squareType.Normal;
        squares[^1] = GameStart.Types.Data.Types.Map.Types.squareType.Normal;

        var map = new GameStart.Types.Data.Types.Map
        {
            Squares = { squares }
        };
        return map;
    }

    public static ServerMessage[]? NextTurn(Client client, TurnEndNotification req)
    {
        var room = DataBaseManager.GetRoom(client.RoomName);
        if (room is null) 
            return null;
        if (client.UserID != room.CurrentTurnPlayerId) 
            return null;
        Console.WriteLine("NextTurn");
        var thisTurnUserID = client.UserID;
        var thisTurnOrder = room.UserOrder[thisTurnUserID];
        var nextTurnOrder = (thisTurnOrder + 1) % room.UserIDs.Count;
        var res = TurnStart(room, nextTurnOrder);
        return res;
    }

    private static ServerMessage[]? TurnStart(Room room, int order)
    {
        if (room.SugorokuMap is null) return null;
        var thisTurnUserID = room.UserOrder.FirstOrDefault(x => x.Value == order).Key;
        var dice = Roulette(1, 6);
        var thisTurnUserPosition = room.UserPosition[thisTurnUserID] + dice;
        room.UserPosition[thisTurnUserID] = thisTurnUserPosition;
        room.CurrentTurnPlayerId = thisTurnUserID;
        DataBaseManager.UpdateRoomData(room);

        if (thisTurnUserID is null) return null;
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
        switch (room.SugorokuMap.Squares[thisTurnUserPosition])
        {
            case GameStart.Types.Data.Types.Map.Types.squareType.Normal:
                break;
            case GameStart.Types.Data.Types.Map.Types.squareType.Quiz:
                var quizArray = QuizService.Quiz(thisTurnUserID);
                var quiz = quizArray[Roulette(0, quizArray.Length - 1)];
                room.QuizData = quiz.QuizStart;
                res.Add(quiz);
                DataBaseManager.UpdateRoomData(room);
                break;
            case GameStart.Types.Data.Types.Map.Types.squareType.Otoshidama:
                var otoshidamaRes = new ServerMessage
                {
                    OtoshidamaEvent = new OtoshidamaEvent
                    {
                        Data = new OtoshidamaEvent.Types.Data
                        {
                            PlayerId = thisTurnUserID,
                            OtoshidamaAmount = Roulette(1, 10) * 1000
                            //Message = "おとし玉イベント"
                        }
                    }
                };
                res.Add(otoshidamaRes);
                break;
            case GameStart.Types.Data.Types.Map.Types.squareType.Furidashi:
                var furidashiRes = new ServerMessage
                {
                    PlayerMovementDisplay = new PlayerMovementDisplay
                    {
                        Data = new PlayerMovementDisplay.Types.Data
                        {
                            PlayerId = thisTurnUserID,
                            NewPosition = 0
                        }
                    }
                };
                room.UserPosition[thisTurnUserID] = 0;
                res.Add(furidashiRes);
                break;
            default:
                throw new ArgumentOutOfRangeException(room.SugorokuMap.Squares[thisTurnUserPosition].GetType().ToString());
        }
        return res.ToArray();
    }

    public static ServerMessage[]? OnGameEndRequest(Client client, GameEndRequest req)
    {
        Console.WriteLine("GameEndRequest");
        var room = DataBaseManager.GetRoom(client.RoomName);
        if (room is null) return null;
        var users = room.UserIDs;
        var score = users.ToDictionary(u => u, u => room.UserOtoshidama[u] + room.UserPosition[u] * 100);
        var scoreRes = new MapField<string, int>
        {
            score
        };
        var res = new ServerMessage
        {
            GameEnd = new GameEnd
            {
                Data = new GameEnd.Types.Data
                {
                    PlayerScores = { scoreRes }
                }
            }
        };
        UniTask.Run(async () =>
        {
            await Task.Delay(5000);
            DataBaseManager.RemoveRoomData(room);
        });
        return [res];
    }
}