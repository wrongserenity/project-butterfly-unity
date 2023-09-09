using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PythonConnector
{
    private readonly Thread receiveThread;
    private bool running;

    private Dictionary<string, float> lastRequestedParsedDict;
    private bool isSentLastRequestedMsg;

    private Dictionary<string, float> lastRecievedDict;

    public PythonConnector()
    {
        receiveThread = new Thread((object callback) =>
        {
            using (var socket = new RequestSocket())
            {
                socket.Connect("tcp://localhost:5555");
                while (running)
                {
                    //socket.SendFrameEmpty();

                    TrySend(socket);
                    Receive(socket);

                    ((Action<Dictionary<string, float>>)callback)(lastRecievedDict);
                }
            }
        });
    }

    void TrySend(RequestSocket socket)
    {
        if (!isSentLastRequestedMsg)
        {
            isSentLastRequestedMsg = true;

            string json = JsonConvert.SerializeObject(lastRequestedParsedDict);
            socket.SendFrame(json);
        }
    }

    void Receive(RequestSocket socket)
    {
        string message = socket.ReceiveFrameString();
        lastRecievedDict = JsonUtility.FromJson<Dictionary<string, float>>(message);
    }

    public void RequestMessageSend(Dictionary<string, float> dict)
    {
        lastRequestedParsedDict = dict;
        isSentLastRequestedMsg = false;
    }

    public Dictionary<string, float> GetLastReceivedData()
    {
        return lastRecievedDict;
    }

    public void Start(Action<Dictionary<string, float>> callback)
    {
        running = true;
        receiveThread.Start(callback);
    }

    public void Stop()
    {
        running = false;
        receiveThread.Join();
    }
}
