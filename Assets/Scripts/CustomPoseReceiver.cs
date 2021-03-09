/*
© Siemens AG, 2017-2018
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
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class CustomPoseReceiver : MessageReceiver
    {
        public enum PoseMessageTypes { GeometryPoseStamped };
        public PoseMessageTypes PoseMessageType;

        private Type type;

        private Vector3 position;
        private Quaternion rotation;
        private bool isMessageReceived;

        public override Type MessageType { get { return type; } }

        private void Awake()
        {
            SetType();
        }

        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }
        private void SetType()
        {
            if (PoseMessageType == PoseMessageTypes.GeometryPoseStamped)
            {
                type = typeof(GeometryPoseStamped);
                MessageReception += ReceiveGeometryPoseStampedMessage;
            }
        }

        private void ReceiveGeometryPoseStampedMessage(object sender, MessageEventArgs e)
        {
            position = GetPosition((GeometryPoseStamped)e.Message);
            rotation = GetRotation((GeometryPoseStamped)e.Message);

            // temporary fix
            Vector3 ros_p;
            ros_p.x = position.x;
            ros_p.y = position.y;
            ros_p.z = position.z;

            Vector3 ros_rot;
            ros_rot.x = rotation.eulerAngles.x;
            ros_rot.y = rotation.eulerAngles.y;
            ros_rot.z = rotation.eulerAngles.z;

            Debug.Log(ros_rot);

            /////////////////////////////////////
            position.x = ros_p.y;
            position.y = ros_p.z;
            position.z = ros_p.x;

            Vector3 tmp_unity_rot = rotation.eulerAngles;
            tmp_unity_rot.x = ros_rot.y;
            tmp_unity_rot.y = -ros_rot.z;
            tmp_unity_rot.z = -ros_rot.x;
            /////////////////////////////////////

            rotation = Quaternion.Euler(tmp_unity_rot.x, tmp_unity_rot.y, tmp_unity_rot.z);

            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        private Vector3 GetPosition(GeometryPoseStamped message)
        {
            return new Vector3(
                message.pose.position.x,
                message.pose.position.y,
                message.pose.position.z);
        }

        private Quaternion GetRotation(GeometryPoseStamped message)
        {
            return new Quaternion(
                message.pose.orientation.x,
                message.pose.orientation.y,
                message.pose.orientation.z,
                message.pose.orientation.w);
        }
    }
}