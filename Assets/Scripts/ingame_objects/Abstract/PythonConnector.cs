using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine.UI;

public class PythonConnector : LoggableBase
{
    [SerializeField] bool isRunning = false;

    [SerializeField] string IP = "127.0.0.1"; 
    [SerializeField] int receivePort = 8000; 
    [SerializeField] int sendPort = 8001;

    [SerializeField] GameObject emotionTextCanvas;
    Text emotionText;

    // Create necessary UdpClient objects
    UdpClient client;
    IPEndPoint remoteEndPoint;

    Thread sendThread;
    Thread receiveThread;

    private Dictionary<string, float> lastRequestedParsedDict;
    private bool isSentLastRequestedMsg;

    private Dictionary<string, float> lastRecievedDict;
    private bool isReceivedDictUpdated;

    private void Start()
    {
        emotionText = emotionTextCanvas.GetComponent<Text>();

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), sendPort);
        client = new UdpClient(receivePort);
    }

    private void FixedUpdate()
    {
        if (isRunning)
            TryUpdateEmotionText();
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
                TryLog("Exeption: " + err.ToString(), LogType.Error);
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
                TryLog("Exeption: " + err.ToString(), LogType.Error);
            }
        }
    }

    void TrySend()
    {
        if (!isSentLastRequestedMsg)
        {
            isSentLastRequestedMsg = true; ;

            string json = JsonConvert.SerializeObject(lastRequestedParsedDict);
            SendUdpMessage(json);
        }
    }

    void SendUdpMessage(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);

            TryLog("Send message: \"" + message + "\"");
        }
        catch (Exception err)
        {
            TryLog("Exeption: " + err.ToString(), LogType.Error);
        }
    }

    void Receive()
    {
        try
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = client.Receive(ref anyIP);
            string text = Encoding.UTF8.GetString(data);

            lastRecievedDict = JsonConvert.DeserializeObject<Dictionary<string, float>>(text);
            isReceivedDictUpdated = true;

            TryLog("Received: " + text);
        }
        catch (Exception err)
        {
            TryLog("Exeption: " + err.ToString(), LogType.Error);
        }
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

    public void StartInternal()
    {
        isRunning = true;

        sendThread = new Thread(new ThreadStart(SendInThread));
        sendThread.IsBackground = true;
        sendThread.Start();

        receiveThread = new Thread(new ThreadStart(ReceiveInThread));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        TryLog("Started python connection");
    }

    public void StopInternal()
    {
        isRunning = false;
        sendThread.Abort();
        receiveThread.Abort();

        SendUdpMessage("stop");
    }

    void TryUpdateEmotionText()
    {
        Dictionary<string, float> receivedData = GetUpdatedData();
        if (receivedData == null)
            return;

        string emotionTextString = "";
        string maxEmotion = "";
        float maxValue = 0.0f;
        foreach (KeyValuePair<string, float> el in receivedData)
        {
            emotionTextString += el.Key + " - " + Math.Round(el.Value, 2) + "\n";

            if (el.Value > maxValue)
            {
                maxValue = el.Value;
                maxEmotion = el.Key;
            }
        }
        emotionTextString += "MAX: " + maxEmotion;

        emotionText.text = emotionTextString;
    }

    void OnDestroy()
    {
        StopInternal();
    }
}
