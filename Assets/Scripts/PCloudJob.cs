using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class PCloudJob : IJob
{
    public PCloud pCloud;
    public SensorPointCloud2 sensorPointCloud2;

    public void Execute()
    {
        pCloud = new PCloud(sensorPointCloud2);
    }
}
