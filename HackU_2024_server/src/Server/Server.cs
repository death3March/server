﻿using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using Cysharp.Threading.Tasks;
using HackU_2024_server.DataBase;
using HackU_2024_server.Service;

namespace HackU_2024_server.Server;

public static class Server
{
    private static bool _isRunning;
    private static readonly TcpListener TcpListener = new(IPAddress.Any, 8080);

    public static async UniTask Start()
    {
        Console.WriteLine("Server Start");

        _isRunning = true;

        TcpListener.Start();

        while (_isRunning)
        {
            var client = await TcpListener.AcceptTcpClientAsync();
            ReceiveHandler(client).Forget();
        }
    }

    private static async UniTask CloseHandler(Client client)
    {
        await UniTask.Run(() =>
        {
            client.Socket?.Dispose();
            client.Socket = null;
            DataBaseManager.UpdateClientData(client);
        });
    }

    private static async UniTask ClientHandler(Client client, byte[] data)
    {
        var recvData = ClientMessage.Parser.ParseFrom(data);
        await EventService.OnReceiveAsync(client, recvData);
    }

    private static async UniTask ReceiveHandler(TcpClient tcpClient)
    {
        var client = new Client();
        await client.InitializeAsync(tcpClient);
        client.StartReceive(ClientHandler, CloseHandler).Forget();
        DataBaseManager.AddClientData(client);
        Console.WriteLine("Client Added");
    }

    public static void Stop()
    {
        _isRunning = false;
    }
}