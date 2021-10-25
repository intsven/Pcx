using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pcx;
using RosSharp.RosBridgeClient;
using System;
using System.Threading;

public class PointCloudManager : MonoBehaviour
{
    public PCloudThread thread = new PCloudThread();
    public float pointSize = 0;

    public void LoadPointCloud(SensorPointCloud2 pCloud)
    {
        // ComputeBuffer container
        // Create a prefab with PointCloudRenderer.

        ImportPointCloudData(pCloud);
        
    }

    public void LoadPointCloud(GameObject go, PCloud pCloud)
    {
        // ComputeBuffer container
        // Create a prefab with PointCloudRenderer.
        var data = ImportPointCloudData(pCloud);
        var renderer = go.GetComponent<PointCloudRenderer>();
        if (!renderer)
        {
            renderer = go.AddComponent<PointCloudRenderer>();

            renderer.pointShader = Shader.Find("Point Cloud/Point");
            renderer.diskShader = Shader.Find("Point Cloud/Disk");

            renderer.pointSize = pointSize;
        }


        Destroy(renderer.sourceData);
        renderer.sourceData = data;
        
    }

    public void FinishImport(GameObject go)
    {
        //PCloud pc = pCloudThread.pCloud;
        var data = ScriptableObject.CreateInstance<PointCloudData>();
        data._pointData = thread.points;
        data.name = "RosCloud";
        var renderer = go.GetComponent<PointCloudRenderer>();
        if (!renderer)
        {
            renderer = go.AddComponent<PointCloudRenderer>();

            renderer.pointShader = Shader.Find("Point Cloud/Point");
            renderer.diskShader = Shader.Find("Point Cloud/Disk");

            renderer.pointSize = pointSize;
        }
        Destroy(renderer.sourceData);
        renderer.sourceData = data;
        

        thread.threadFinished = false;
    }

    public void StartThread()
    {
        Thread t = new Thread(new ThreadStart(thread.CreatePCloud));
        thread.thread = t;
        t.Start();
    }

    private void OnDestroy()
    {
        thread.thread.Abort();
    }

    private void ImportPointCloudData(SensorPointCloud2 pCloud)
    {
        try
        {
            thread.pcQueue.Enqueue(pCloud);

        }
        catch (Exception e)
        {
            Debug.LogError("Failed importing data " + e.Message);
        }
    }


    private PointCloudData ImportPointCloudData(PCloud pCloud)
    {
        try
        {
            //var data = ScriptableObject.CreateInstance<PointCloudData>();
            var data = new PointCloudData();
            data.Initialize(pCloud.vertices, pCloud.colors);
            data.name = "RosCloud";
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed importing data " + e.Message);
            return null;
        }
    }
}
