using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pcx;
using System;
using static Pcx.PointCloudData;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using System.Text;

namespace Awesome.Pointcloud
{

    public class TestPC : WSClient
    {
        PointCloudRenderer[] pcrenderer;
        public PointCloudRenderer sourceRenderer;
        PointCloudData pointCloudData = null;
        const int instances = 3;
        ComputeBuffer[] buffer = new ComputeBuffer[instances];
        Point[] array;
        int stride;
        IFormatter formatter;
        int pointCount;
        string dataPath;
        System.Diagnostics.Stopwatch watch;
        PointCloudData[] data = new PointCloudData[instances];
        byte[][] bytes = new byte[instances][];
        int rendCount;
        Thread[] threads;
        bool[] finished = new bool[instances];
        int numPoints;
        // Start is called before the first frame update
        new void Start()
        {
            base.Start();
            pcrenderer = GetComponentsInChildren<PointCloudRenderer>();
            //pcrenderer[0].gameObject.SetActive(false);
            array = new Point[1];
            stride = 16;
            pointCount = 238154;
            numPoints = pointCount / 8;
            dataPath = Application.dataPath;
            byte[] managedArray = new byte[numPoints * 16];
            byte[] singleArray = new byte[pointCount * 16];
            sourceRenderer.sourceData.computeBuffer.GetData(singleArray);
            //Array.Copy(singleArray, managedArray, managedArray.Length);
            var multiplier = pointCount / numPoints;
            for (int i = 0; i < managedArray.Length; i+= 16)
            {
                //managedArray[i] = singleArray[i * multiplier];
                Array.Copy(singleArray, i * multiplier, managedArray, i, 16);
            }
            //Array.Copy(singleArray, 0, managedArray, singleArray.Length, singleArray.Length);
            //Array.Copy(singleArray, 0, managedArray, singleArray.Length * 2, singleArray.Length);
            //Marshal.Copy(pntr, managedArray, 0, byteCount);
            File.WriteAllBytes(Path.Combine(Application.dataPath, "pointcloud.bin"), managedArray);

            var point = sourceRenderer.sourceData._pointData[pointCount - pointCount % (16)];
            Debug.Log(point.position);
            Debug.Log(ByteArrayToString(BitConverter.GetBytes(point.color)));
            Debug.Log(ByteArrayToString(BitConverter.GetBytes(point.position.x)));
            Debug.Log(ByteArrayToString(BitConverter.GetBytes(point.position.y)));
            Debug.Log(ByteArrayToString(BitConverter.GetBytes(point.position.z)));

            //ThreadStart childref = new ThreadStart(loadPC);
            //Thread childThread = new Thread(childref);
            //childThread.Start();
            threads = new Thread[instances];
            for (int i = 0; i < instances; i++)
            {
                var j = i;
                var t = new Thread(() => loadPC(j));
                threads[i] = t;
                t.Start();
                finished[i] = false;
                bytes[i] = File.ReadAllBytes(Path.Combine(dataPath, "pointcloud.bin"));
            }
            

            //Debug.unityLogger.logEnabled = false;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        public static void writePCtoFile(PointCloudData sourceData, string file)
        {
            int pointCount = sourceData.pointCount;
            int numPoints = pointCount;
            var dataPath = Application.dataPath;
            byte[] managedArray = new byte[numPoints * 16];
            byte[] singleArray = new byte[pointCount * 16];
            sourceData.computeBuffer.GetData(singleArray);
            //Array.Copy(singleArray, managedArray, managedArray.Length);
            var multiplier = pointCount / numPoints;
            for (int i = 0; i < managedArray.Length; i += 16)
            {
                //managedArray[i] = singleArray[i * multiplier];
                Array.Copy(singleArray, i * multiplier, managedArray, i, 16);
            }
            //Array.Copy(singleArray, 0, managedArray, singleArray.Length, singleArray.Length);
            //Array.Copy(singleArray, 0, managedArray, singleArray.Length * 2, singleArray.Length);
            //Marshal.Copy(pntr, managedArray, 0, byteCount);
            File.WriteAllBytes(Path.Combine(Application.dataPath, file), managedArray);
        }


        void Update()
        {
            pcrenderer[(rendCount + 2) % instances].renderingEnabled = true;
            pcrenderer[(rendCount + 2) % instances].gameObject.SetActive(true);
            for (int i = 0; i < instances; i++)
            {
                if(finished[i])
                {
                    if (pcrenderer[i]._diskMaterial)
                    {
                        pcrenderer[i]._diskMaterial.SetBuffer("_PointBuffer", buffer[i]);
                    }
                        
                    finished[i] = false;
                    break;
                }
            }
            
            rendCount = (rendCount + 1) % instances;
            //StartCoroutine(loadPC());
            //Debug.unityLogger.logEnabled = false;
            watch = System.Diagnostics.Stopwatch.StartNew();
            //if (buffer[rendCount] != null)
            //    buffer[rendCount].Release();
            if(buffer[rendCount] == null)
                buffer[rendCount] = new ComputeBuffer(numPoints * 16, stride);
            //bytes[rendCount] = File.ReadAllBytes(Path.Combine(dataPath, "pointcloud.bin"));
            Debug.Log("file.Read(bytes;" + watch.ElapsedMilliseconds);
            buffer[rendCount].SetData(bytes[rendCount]);
            Debug.Log("buffer.SetData(bytes);;" + watch.ElapsedMilliseconds);
            pcrenderer[rendCount].gameObject.SetActive(false);
            pcrenderer[rendCount].renderingEnabled = false;
            //DestroyImmediate(pcrenderer[rendCount].sourceData);
            data[rendCount] = ScriptableObject.CreateInstance<PointCloudData>();
            Debug.Log("ScriptableObject;" + watch.ElapsedMilliseconds);
            //data[rendCount].computeBuffer = buffer[rendCount];
            Debug.Log("data.computeBuffer = buffer;;" + watch.ElapsedMilliseconds);
            //pcrenderer[rendCount].sourceData = null;
            //pcrenderer[rendCount].sourceData = data[rendCount];
            //pcrenderer[rendCount]._diskMaterial.SetBuffer("_PointBuffer", buffer[rendCount]);
            //pcrenderer[rendCount].gameObject.SetActive(true);
            //data[rendCount]._pointData = new Point[buffer[rendCount].count / elementSize];
            //data[rendCount].computeBuffer.GetData(data[rendCount]._pointData);
            if (threads[rendCount].ThreadState == ThreadState.WaitSleepJoin)
                threads[rendCount].Interrupt();
            
            Debug.Log("test2" + watch.ElapsedMilliseconds);
        }

        void OnDestroy()
        {
            for (int i = 0; i < instances; i++)
            {
                threads[i].Abort();
            }
        }

        // Update is called once per frame
        void loadPC(int i)
        {
            try
            {
                Thread.Sleep(Timeout.Infinite);
            }
            catch (ThreadInterruptedException) { }
            while (true) 
                if (array != null) {
                    Debug.Log("test");
                    //Debug.unityLogger.logEnabled = false;
                
                    //pcrenderer.sourceBuffer = null;
                
                    Debug.Log("buffer.Dispose();" + watch.ElapsedMilliseconds);




                    Debug.Log("threadi" + i);
                    //pcrenderer[i].sourceData = data[i];
                    data[i].computeBuffer = buffer[i];
                    Debug.Log("data.computeBuffer = buffer;;" + watch.ElapsedMilliseconds);
                    //pcrenderer.sourceData = null;
                    pcrenderer[i].sourceData = data[i];
                    //Debug.Log(array[0].position);
                    //pcrenderer.sourceBuffer = buffer;
                    //array = null;

                    Debug.Log("array = null" + watch.ElapsedMilliseconds);
                    //Debug.unityLogger.logEnabled = true;
                    Debug.Log("Debug.unityLogger.logEnabled = true;" + watch.ElapsedMilliseconds);
                    //bytes[rendCount] = File.ReadAllBytes(Path.Combine(dataPath, "pointcloud.bin"));
                    //pcrenderer[i].renderingEnabled = true;
                    finished[i] = true;
                    try
                    {
                        Thread.Sleep(1000);
                    }
                    catch (ThreadInterruptedException) { }
                    
                }


        }

        protected override void OnJSONMessage(JObject msg)
        {
            
        }

        protected override void OnWSMessage(object sender, MessageEventArgs e)
        {
            //bytes[0] = Encoding.ASCII.GetBytes(e.Data.ToCharArray());
            bytes[0] = e.RawData;
            //Debug.Log(e.Data);
            Debug.Log("OnWSMessage: " + e.RawData.Length);
            for (int i = 1; i < instances; i++)
            {
                bytes[i] = bytes[0];
            }
        }



        public class Vector3SerializationSurrogate : ISerializationSurrogate
        {

            // Method called to serialize a Vector3 object
            public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
            {

                Vector3 v3 = (Vector3)obj;
                info.AddValue("x", v3.x);
                info.AddValue("y", v3.y);
                info.AddValue("z", v3.z);
            }

            // Method called to deserialize a Vector3 object
            public System.Object SetObjectData(System.Object obj, SerializationInfo info,
                                            StreamingContext context, ISurrogateSelector selector)
            {

                Vector3 v3 = (Vector3)obj;
                v3.x = (float)info.GetValue("x", typeof(float));
                v3.y = (float)info.GetValue("y", typeof(float));
                v3.z = (float)info.GetValue("z", typeof(float));
                obj = v3;
                return obj;
            }
        }

        byte[] getBytes(Point[] str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        Point[] fromBytes(byte[] arr)
        {
            Point[] str = new Point[pointCount];

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (Point[])Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }
        /*
        void OldStart()
        {

            Debug.Log(pcrenderer.sourceData.computeBuffer);

            //pointCloudData = pcrenderer.sourceData;
            //buffer = pcrenderer.sourceData.computeBuffer;
            var length = pcrenderer.sourceData.pointCount;
            pointCount = length;
            var newArray = new Point[length];
            pcrenderer.sourceData.computeBuffer.GetData(newArray);
            var pntr = pcrenderer.sourceData.computeBuffer.GetNativeBufferPtr();
            array = new Point[length];

            Array.Copy(newArray, array, array.Length);
            Debug.Log(array.Length);
            stride = pcrenderer.sourceData.computeBuffer.stride;

            formatter = new BinaryFormatter();
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();

            surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
            formatter.SurrogateSelector = surrogateSelector;
            using (FileStream stream = new FileStream(Path.Combine(Application.dataPath, "pointcloud.bin"), FileMode.Create))
            {
                //formatter.Serialize(stream, array);

            }
            //int byteCount = pcrenderer.sourceData.computeBuffer.count;
            byte[] managedArray = new byte[pointCount * 16];
            pcrenderer.sourceData.computeBuffer.GetData(managedArray);
            //Marshal.Copy(pntr, managedArray, 0, byteCount);
            File.WriteAllBytes(Path.Combine(Application.dataPath, "pointcloud.bin"), managedArray);

        }
        
        void OldUpdate()
        {
            if (array != null)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                pcrenderer.sourceBuffer = null;
                if (buffer != null)
                    buffer.Release();
                Debug.Log("buffer.Dispose();" + watch.ElapsedMilliseconds);

                byte[] bytes = File.ReadAllBytes(Path.Combine(Application.dataPath, "pointcloud.bin"));
                Debug.Log("file.Read(bytes;" + watch.ElapsedMilliseconds);
                //var file = File.Open(Path.Combine(Application.dataPath, "pointcloud.bin"), FileMode.Open);
                Debug.Log("File.Open" + watch.ElapsedMilliseconds);

                var stream = new MemoryStream(bytes);
                Debug.Log("new MemoryStream(bytes); " + watch.ElapsedMilliseconds);
                //array = (Point[])formatter.Deserialize(stream);
                //array = fromBytes(bytes);
                Debug.Log("formatter.Deserialize;" + watch.ElapsedMilliseconds);
                //file.Close();
                Debug.Log("file.Close();" + watch.ElapsedMilliseconds);
                buffer = new ComputeBuffer(pointCount * 16, stride);
                buffer.SetData(bytes);
                Debug.Log("buffer.SetData(bytes);;" + watch.ElapsedMilliseconds);
                DestroyImmediate(pcrenderer.sourceData);
                var data = ScriptableObject.CreateInstance<PointCloudData>();
                Debug.Log("ScriptableObject;" + watch.ElapsedMilliseconds);
                data.computeBuffer = buffer;
                Debug.Log("data.computeBuffer = buffer;;" + watch.ElapsedMilliseconds);
                pcrenderer.sourceData = null;
                pcrenderer.sourceData = data;
                //Debug.Log(array[0].position);
                //pcrenderer.sourceBuffer = buffer;
                array = null;

                Debug.Log("array = null" + watch.ElapsedMilliseconds);
            }



        }*/
    }
}
