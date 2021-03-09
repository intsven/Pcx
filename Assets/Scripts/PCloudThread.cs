using Pcx;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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
            SensorPointCloud2 sensorPointCloud2 = pcQueue.Dequeue();
            pCloud = new PCloud(sensorPointCloud2);
            points = Initialize(pCloud.vertices, pCloud.colors);
            threadFinished = true;
            Debug.Log("test");
        }
    }

    static uint EncodeColor(Color c)
    {
        const float kMaxBrightness = 16;

        var y = Mathf.Max(Mathf.Max(c.r, c.g), c.b);
        y = Mathf.Clamp(Mathf.Ceil(y * 255 / kMaxBrightness), 1, 255);

        var rgb = new Vector3(c.r, c.g, c.b);
        rgb *= 255 * 255 / (y * kMaxBrightness);

        return ((uint)rgb.x) |
               ((uint)rgb.y << 8) |
               ((uint)rgb.z << 16) |
               ((uint)y << 24);
    }

    public PointCloudData.Point[] Initialize(List<Vector3> positions, List<Color32> colors)
    {
        PointCloudData.Point[]  _pointData = new PointCloudData.Point[positions.Count];
        for (var i = 0; i < _pointData.Length; i++)
        {
            _pointData[i] = new PointCloudData.Point
            {
                position = positions[i],
                color = EncodeColor(colors[i])
            };
        }
        return _pointData;
    }
}
