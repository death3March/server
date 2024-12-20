﻿using MasterMemory;
using MessagePack;

namespace HackU_2024_server.DataBase;

[MemoryTable("Room")]
[MessagePackObject(true)]
public class Room
{
    public enum RoomState
    {
        Waiting,
        Gaming,
        Result
    }

    [PrimaryKey] public string RoomName { get; set; } = string.Empty;
    public List<string> UserIDs { get; set; } = [];
    public Dictionary<string, int> UserOrder { get; set; } = new();
    public Dictionary<string, int> UserOtoshidama { get; set; } = new();
    public Dictionary<string, int> UserPosition { get; set; } = new();
    public Dictionary<string, bool> UserIsAnswered { get; set; } = new();
    public Dictionary<string, int?> UserIsAnsweredOrder { get; set; } = new();
    public Dictionary<string, string> UserAnswer { get; set; } = new();
    public QuizStart? QuizData { get; set; } = null;
    public RoomState State { get; set; } = RoomState.Waiting;
    public GameStart.Types.Data.Types.Map? SugorokuMap { get; set; } = null;
    public string CurrentTurnPlayerId { get; set; } = string.Empty;
}