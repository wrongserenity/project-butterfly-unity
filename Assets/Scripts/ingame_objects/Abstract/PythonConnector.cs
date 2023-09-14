
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class PythonConnector
{
    bool isRunning = false;

    string IP = "127.0.0.1"; // local host
    int rxPort = 8000; // port to receive data from Python on
    int txPort = 8001; // port to send data to Python on

    // Create necessary UdpClient objects
    UdpClient client;
    IPEndPoint remoteEndPoint;

    Thread sendThread;
    Thread receiveThread;

    private Dictionary<string, float> lastRequestedParsedDict;
    private bool isSentLastRequestedMsg;

    private Dictionary<string, float> lastRecievedDict;
    private bool isReceivedDictUpdated;

    public PythonConnector()
    {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), txPort);

        client = new UdpClient(rxPort);
    }

    void SendInThread()
    {
        while (isRunning)
        {
            try
            {
                TrySend();

                Thread.Sleep(200);
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

    void ReceiveInThread()
    {
        while (isRunning)
        {
            try
            {
                Receive();
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

    void TrySend()
    {
        if (!isSentLastRequestedMsg)
        {
            isSentLastRequestedMsg = true;;

            string json = JsonConvert.SerializeObject(lastRequestedParsedDict);
            SendMessage(json);
        }
    }

    void SendMessage(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            Debug.Log(err.ToString());
        }
    }

    void Receive()
    {
        try
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = client.Receive(ref anyIP);
            string text = Encoding.UTF8.GetString(data);
            Debug.Log(">> " + text);

            // lastRecievedDict = JsonUtility.FromJson<Dictionary<string, float>>(text.Replace("\\", ""));
            lastRecievedDict = JsonConvert.DeserializeObject<Dictionary<string, float>>(text);
            isReceivedDictUpdated = true;
        }
        catch (Exception err)
        {
            Debug.Log(err.ToString());
        }

        /*string message = socket.ReceiveFrameString();
        lastRecievedDict = JsonUtility.FromJson<Dictionary<string, float>>(message);

        Debug.Log("received " + lastRecievedDict["value"].ToString());*/
    }

    public void RequestMessageSend(Dictionary<string, float> dict)
    {
        lastRequestedParsedDict = dict;
        isSentLastRequestedMsg = false;
    }

    public Dictionary<string, float> GetUpdatedData()
    {
        if (isReceivedDictUpdated)
        {
            isReceivedDictUpdated = false;
            return lastRecievedDict;
        }
        return null;
    }

    public void Start()
    {
        isRunning = true;

        sendThread = new Thread(new ThreadStart(SendInThread));
        sendThread.IsBackground = true;
        sendThread.Start();

        receiveThread = new Thread(new ThreadStart(ReceiveInThread));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    public void Stop()
    {
        isRunning = false;
        sendThread.Abort();
        receiveThread.Abort();

        SendMessage("stop");
    }
}