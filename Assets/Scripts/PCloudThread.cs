using Pcx;
using RosSharp.RosBridgeClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Awesome.Pointcloud;

public class PCloudThread
{
    public PCloud pCloud;
    public PointCloudData.Point[] points;

    public bool threadFinished = false;

    public Queue<SensorPointCloud2> pcQueue = new Queue<SensorPointCloud2>();

    public Thread thread;

    public PCloudThread()
    {
        threadFinished = false;
    }

    public void CreatePCloud()
    {
        while(true)
        {
            while (pcQueue.Count == 0) ;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SensorPointCloud2 sensorPointCloud2 = pcQueue.Dequeue();
            UnityEngine.Debug.Log("pcQueue.Dequeue(): " + sw.Elapsed.TotalMilliseconds);
            pCloud = new PCloud(sensorPointCloud2);
            UnityEngine.Debug.Log("new PCloud: " + sw.Elapsed.TotalMilliseconds);
            points = Initialize(pCloud.vertices, pCloud.colors);
            UnityEngine.Debug.Log("Initialize: " + sw.Elapsed.TotalMilliseconds);
            threadFinished = true;
        }
    }

    static uint EncodeColor(Color c, int i)
    {
        const float kMaxBrightness = 16;

        var y = Mathf.Max(Mathf.Max(c.r, c.g), c.b);
        y = Mathf.Clamp(Mathf.Ceil(y * 255 / kMaxBrightness), 1, 255);
        if (i == 0)
            UnityEngine.Debug.Log(y);

        var rgb = new Vector3(c.r, c.g, c.b);
        rgb *= 255 * 255 / (y * kMaxBrightness);

        //UnityEngine.Debug.Log(c.r + " " + TestPC.ByteArrayToString(BitConverter.GetBytes((uint)rgb.x)));

        var ret = ((uint)rgb.x) |
               ((uint)rgb.y << 8) |
               ((uint)rgb.z << 16) |
               ((uint)y << 24);
        if (i == 0)
        {
            UnityEngine.Debug.Log(TestPC.ByteArrayToString(BitConverter.GetBytes(ret)) + "  " + rgb.x);
        }
            
        return ret;
    }

    static uint EncodeColor2(Color c)
    {
        return ((uint)(c.r * 255)) |
               ((uint)(c.g * 255) << 8) |
               ((uint)(c.b * 255) << 16) |
               ((uint)(c.a * 255) << 24);
    }

    public PointCloudData.Point[] Initialize(List<Vector3> positions, List<Color32> colors)
    {
        PointCloudData.Point[]  _pointData = new PointCloudData.Point[positions.Count];
        for (var i = 0; i < _pointData.Length; i++)
        {
            _pointData[i] = new PointCloudData.Point
            {
                position = positions[i],
                color = EncodeColor(colors[i], i)
            };
        }
        return _pointData;
    }
}
