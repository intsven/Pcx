using Newtonsoft.Json.Linq;
using Pcx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using WebSocketSharp;

namespace Awesome.Pointcloud
{
    [RequireComponent(typeof(PointCloudRenderer))]
    public class WSPointCloud : WSClient
    {
        byte[] data;
        bool applied = false;
        PointCloudRenderer renderer;
        ComputeBuffer buffer;
        // Start is called before the first frame update
        public new void Start()
        {
            base.Start();
            renderer = GetComponent<PointCloudRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (data != null && data.Length > 0 && !applied)
            {
                buffer = new ComputeBuffer(data.Length, 16);
                buffer.SetData(data);
                var pcdata = ScriptableObject.CreateInstance<PointCloudData>();
                renderer.sourceData = pcdata;
                //data = null;
                applied = true;
            }
        }

        protected override void OnWSMessage(object sender, MessageEventArgs e)
        {
            try
            {
                //Debug.Log(e.Data);
                data = e.RawData;
            }
            catch (System.Exception exc)
            {

                Debug.Log(exc);
            }

        }
    }
}
