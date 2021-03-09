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

    public PCloud(RosSharp.RosBridgeClient.SensorPointCloud2 sensorPointCloud2)
    {
        int I = sensorPointCloud2.data.Length / sensorPointCloud2.point_step;
        byte[] byteSlice = new byte[sensorPointCloud2.point_step];

        float x = 0.0f, y = 0.0f, z = 0.0f;
        byte r = 0, g = 0, b = 0, a = 0;

        for (int i = 0; i < I; i+=pointFilter)
        {
            Array.Copy(sensorPointCloud2.data, i * sensorPointCloud2.point_step, byteSlice, 0, sensorPointCloud2.point_step);

            foreach (var f in sensorPointCloud2.fields)
            {
                byte[] slice = new byte[f.count * 4];
                Array.Copy(byteSlice, f.offset, slice, 0, f.count * 4);

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
                }
            }

            // fixed transformation between kinect ref. system and unity
            vertices.Add(new Vector3(x, -y, z));
            colors.Add(new Color32(r, g, b, a));
        }
    }

    private static float getValue(byte[] bytes)
    {
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        float result = BitConverter.ToSingle(bytes, 0);
        return result;
    }
}