/*
© Siemens AG, 2017
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using UnityEngine;

public class PCloud
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<Color32> colors = new List<Color32>();
    public const int pointFilter = 1;

    public PCloud(RosSharp.RosBridgeClient.SensorPointCloud2 sensorPointCloud2, bool old)
    {
        int I = sensorPointCloud2.data.Length / sensorPointCloud2.point_step;
        byte[] byteSlice = new byte[sensorPointCloud2.point_step];

        float x = 0.0f, y = 0.0f, z = 0.0f;
        byte r = 0, g = 0, b = 0, a = 0;
        Dictionary<string, int> names = new Dictionary<string, int>();
        float maxIntensity = 0;

        for (int i = 0; i < I; i+=pointFilter)
        {
            Array.Copy(sensorPointCloud2.data, i * sensorPointCloud2.point_step, byteSlice, 0, sensorPointCloud2.point_step);
            byte byteIntensity = 0;
            foreach (var f in sensorPointCloud2.fields)
            {
                byte[] slice = new byte[f.count * 4];
                Array.Copy(byteSlice, f.offset, slice, 0, f.count * 4);

                if (!names.ContainsKey(f.name))
                    names.Add(f.name, 0);
                names[f.name] += 1;

                

                switch (f.name)
                {
                    case "x":
                        x = getValue(slice);
                        break;
                    case "y":
                        y = getValue(slice);
                        break;
                    case "z":
                        z = getValue(slice);
                        break;
                    case "rgb":
                        r = slice[2];
                        g = slice[1];
                        b = slice[0];
                        a = 1;
                        break;
                    case "reflectivity":
                        float intensity = getValue(slice);
                        if (!names.ContainsKey("datatype"))
                            names.Add("datatype", f.datatype);

                        //if (!names.ContainsKey("intensity" + intensity))
                        //    names.Add("intensity" + intensity, 0);
                        //names["intensity" + intensity] += 1;

                        if (intensity > maxIntensity)
                            maxIntensity = intensity;
                        byteIntensity = (byte) Mathf.RoundToInt(intensity / 65526 * 255f);
                        a = 255;

                        //if (!names.ContainsKey("intensity" + r))
                        //    names.Add("intensity" + r, 0);
                        //names["intensity" + r] += 1;
                        break;
                    default:
                        break;
                }


            }
            r = byteIntensity;
            g = byteIntensity;
            b = byteIntensity;

            // fixed transformation between kinect ref. system and unity
            vertices.Add(new Vector3(x, -y, z));
            colors.Add(new Color32(r, g, b, a));
        }
        Debug.Log(String.Join(" ", names));
        Debug.Log("maxIntensity " + maxIntensity);
    }

    public PCloud(RosSharp.RosBridgeClient.SensorPointCloud2 sensorPointCloud2)
    {
        int I = sensorPointCloud2.data.Length / sensorPointCloud2.point_step;
        byte[] byteSlice = new byte[sensorPointCloud2.point_step];

        float x = 0.0f, y = 0.0f, z = 0.0f;
        byte r = 0, g = 0, b = 0, a = 0;
        Dictionary<string, int> names = new Dictionary<string, int>();
        float maxIntensity = 0;

        for (int i = 0; i < I; i += pointFilter)
        {
            //Array.Copy(sensorPointCloud2.data, i * sensorPointCloud2.point_step, byteSlice, 0, sensorPointCloud2.point_step);
            byte byteIntensity = 0;
            foreach (var f in sensorPointCloud2.fields)
            {
                //byte[] slice = new byte[f.count * 4];
                //Array.Copy(sensorPointCloud2.data, i * sensorPointCloud2.point_step + f.offset, slice, 0, f.count * 4);

                if (!names.ContainsKey(f.name))
                    names.Add(f.name, 0);
                names[f.name] += 1;



                switch (f.name)
                {
                    case "x":
                        x = getValue(sensorPointCloud2.data, i * sensorPointCloud2.point_step + f.offset);
                        break;
                    case "y":
                        y = getValue(sensorPointCloud2.data, i * sensorPointCloud2.point_step + f.offset);
                        break;
                    case "z":
                        z = getValue(sensorPointCloud2.data, i * sensorPointCloud2.point_step + f.offset);
                        break;
                    case "rgb":
                        r = sensorPointCloud2.data[i * sensorPointCloud2.point_step + f.offset + 2];
                        g = sensorPointCloud2.data[i * sensorPointCloud2.point_step + f.offset + 1];
                        b = sensorPointCloud2.data[i * sensorPointCloud2.point_step + f.offset + 0];
                        a = 1;
                        break;
                    case "reflectivity":
                        float intensity = getValue(sensorPointCloud2.data, i * sensorPointCloud2.point_step + f.offset);
                        if (!names.ContainsKey("datatype"))
                            names.Add("datatype", f.datatype);

                        //if (!names.ContainsKey("intensity" + intensity))
                        //    names.Add("intensity" + intensity, 0);
                        //names["intensity" + intensity] += 1;

                        if (intensity > maxIntensity)
                            maxIntensity = intensity;
                        byteIntensity = (byte)Mathf.RoundToInt(intensity / 65526 * 255f);
                        a = 255;

                        if (i == 0)
                            Debug.Log("starintensity: " + intensity);

                        //if (!names.ContainsKey("intensity" + r))
                        //    names.Add("intensity" + r, 0);
                        //names["intensity" + r] += 1;
                        break;
                    default:
                        break;
                }


            }
            r = byteIntensity;
            g = byteIntensity;
            b = byteIntensity;

            // fixed transformation between kinect ref. system and unity
            vertices.Add(new Vector3(x, -y, z));
            colors.Add(new Color32(r, g, b, a));
        }
        Debug.Log(String.Join(" ", names));
        Debug.Log("maxIntensity " + maxIntensity);
    }

    private static float getValue(byte[] bytes)
    {
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        float result = BitConverter.ToSingle(bytes, 0);
        return result;
    }

    private static float getValue(byte[] bytes, int offset)
    {
        //if (!BitConverter.IsLittleEndian)
        //    Array.Reverse(bytes);

        float result = BitConverter.ToSingle(bytes, offset);
        return result;
    }
}