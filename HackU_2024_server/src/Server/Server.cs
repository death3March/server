using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using HackU_2024_server.DataBase;

namespace HackU_2024_server.Server;

public static class Server
{
    private static bool _isRunning = false;
    private static readonly HttpListener HttpListener = new();
    
    public static async UniTask Start()
    {
        _isRunning = true;
        
        HttpListener.Prefixes.Add("http://+:8080/");
        HttpListener.Start();

        while (_isRunning)
        {
            var context = await HttpListener.GetContextAsync();
            if (!context.Request.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;	// bad request
                context.Response.Close();
                continue;
            }
            ReceiveHandler(context).Forget();
        }
    }

    private static async UniTask ReceiveHandler(HttpListenerContext context)
    {
        var client = new ClientData();
        await client.InitializeAsync(context);
        // TODO:save client obj to MasterMemory
    }

    public static void Stop()
    {
        _isRunning = false;
        HttpListener.Stop();
        HttpListener.Close();
    }
}