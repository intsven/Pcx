using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utils;
using System.Drawing;

public class WSText : WSClient
{
    string data;

    public TextAsset text;

    private new void Start()
    {
        /*
        base.Start();
        Debug.Log(text.text);
        byte[] bytes = Convert.FromBase64String(text.text);
        /*length = len(string)
        width = floor(sqrt(length/3.0))
        height = ceil((length/3.0) / width)
        bytes_needed = int(width * height * 3)

        int width = (int)Mathf.Sqrt(bytes.Length / 3f);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        Debug.Log(BitConverter.ToString(tex.GetRawTextureData()));
        GetComponent<Renderer>().material.mainTexture = tex;

        byte[] buffer = Convert.FromBase64String(text.text);
        string ascii = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
        JObject jsonElement = JsonUtility.FromJson<JObject>(ascii);
        Debug.Log(jsonElement);*/
    }
    
    protected override void OnJSONMessage(JObject msg)
    {
        var token = msg.Get("data");
        if (token.Type != JTokenType.Object)
            data = token.ToString();
        else
            data = "data is an object: \n" + token.ToString();
    }

    public void Update()
    {
        if(data != null)
        {
            byte[] bytes = Convert.FromBase64String(data);
            
            data = null;
        }
        
    }
}
