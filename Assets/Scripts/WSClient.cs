using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using Newtonsoft.Json.Linq;

public class WSClient : MonoBehaviour
{
    WebSocket ws;

    [SerializeField]
    protected string address;
    [SerializeField]
    protected string path;

    public void Start()
    {
        StartClient();
    }

    public void StartClient(string address, string path)
    {
        this.address = address;
        this.path = path;
        StartClient();
    }


    public void StartClient()
    {
        if (ws != null)
            ws.Close();

        ws = new WebSocket(address);
        ws.Compression = CompressionMethod.Deflate;

        ws.OnMessage += OnWSMessage;

        ws.OnError += (sender, e) =>
        {
            Debug.Log("Error " + ((WebSocket)sender).Url + ", Data : " + e);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("close " + ((WebSocket)sender).Url + ", Data : " + e.Code);
        };

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("open " + ((WebSocket)sender).Url + ", Data : " + e);
            var message = new { path = path};            

            string json = JsonConvert.SerializeObject(message);
            Debug.Log(json);
            ws.Send(json); 
        };

        //ws.Connect();
        ws.ConnectAsync();
    }

    protected virtual void OnWSMessage(object sender, MessageEventArgs e)
    {
        try
        {
            Debug.Log(e.Data);
            JObject msg = JsonConvert.DeserializeObject<JObject>(e.Data);
            OnJSONMessage(msg);
        }
        catch (System.Exception exc)
        {

            Debug.Log(exc);
        }
        
    }

    protected virtual void OnJSONMessage(JObject msg)
    {
        Debug.Log("Message Received from " + ws.Url + ", Data : " + msg);
    }


    private void Update()
    {
        
    }

    private void OnDestroy()
    {
        if(ws != null)
            ws.Close();
    }
}

namespace Utils
{
    public static class MyExtensions
    {
        public static JToken Get(this JObject obj, string path)
        {
            var segments = path.Trim('/').Split('/');
            JToken token = obj;
            foreach(var s in segments)
            {
                token = token[s];
                if (token == null)
                    return JValue.CreateNull();
            }
            return token;
        }

        public static JToken Get(this JToken obj, string path)
        {
            return ((JObject)obj).Get(path);
        }
    }
}