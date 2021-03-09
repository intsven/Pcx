using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Pcx;

namespace RosSharp.RosBridgeClient
{
    public class PointCloudReceiver : MessageReceiver
    {
        public PointCloudManager pManager;
        public Transform pContainer;

        private bool isMessageReceived;
        private PCloud pCloud;
        private SensorPointCloud2 msg;

        private GameObject[] gos = new GameObject[2];

        int curGO = 0;

        public override Type MessageType { get { return (typeof(SensorPointCloud2)); } }

        public int queueSize = 0;

        GameObject lastGo;


        private void Awake()
        {
            MessageReception += ReceiveMessage;
            pManager.StartThread();
        }

        private void Start()
        {
            for(int i = 0; i < gos.Length; ++i)
            {
                //GameObject.Destroy(go);
                gos[i] = new GameObject();
                GameObject go = gos[i];
                go.transform.SetParent(pContainer);
                go.transform.localPosition = new Vector3(0, 0, 0);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        private void ReceiveMessage(object sender, MessageEventArgs e)
        {
            //UnityEngine.Debug.Log("ReceiveMessage: Start -> " + System.DateTime.Now.ToLongTimeString());

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //pCloud = new PCloud((SensorPointCloud2)e.Message);
            
            msg = (SensorPointCloud2)e.Message;
            isMessageReceived = true;

            sw.Stop();
            UnityEngine.Debug.Log("ReceiveMessage Elapsed: " + sw.Elapsed.TotalMilliseconds);

            //UnityEngine.Debug.Log("ReceiveMessage: Done  -> " + System.DateTime.Now.ToLongTimeString());
        }

        GameObject getCurGO()
        {
            GameObject go = gos[curGO];
            curGO = (curGO + 1) % gos.Length;
            return go;
        }

        // Update is called once per frame
        void Update()
        {
            if (isMessageReceived)
            {
                //UnityEngine.Debug.Log("Update: Start -> " + System.DateTime.Now.ToLongTimeString());

                Stopwatch sw = new Stopwatch();
                sw.Start();

                ProcessMessage();

                sw.Stop();
                UnityEngine.Debug.Log("ProcessMessage : " + sw.Elapsed.TotalMilliseconds);
                //UnityEngine.Debug.Log("Update: Done -> " + System.DateTime.Now.ToLongTimeString());
            }

            queueSize = pManager.thread.pcQueue.Count;

            if (lastGo)
            {
                lastGo.GetComponent<PointCloudRenderer>().enabled = false;
                lastGo = null;
            }
                


            if (pManager.thread.threadFinished)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                //gos[curGO].SetActive(false);
                

                lastGo = gos[curGO];


                pManager.FinishImport(getCurGO());
                if(gos[curGO].GetComponent<PointCloudRenderer>())
                    gos[curGO].GetComponent<PointCloudRenderer>().enabled = true;
                
                //gos[curGO].SetActive(true);
                sw.Stop();
                UnityEngine.Debug.Log("FinishImport: " + sw.Elapsed.TotalMilliseconds);
            }
            
            
                
            
        }

        

        private void ProcessMessage()
        {
            





            //pManager.LoadPointCloud(go, pCloud);
            pManager.LoadPointCloud(msg);
            isMessageReceived = false;
        }
    }
}