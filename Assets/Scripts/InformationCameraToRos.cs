using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public class InformationCameraToRos : MonoBehaviour
    {
        private RosSocket rosSocket;
        public string topic = "/message";
        private GeometryPose MessagePose;
        private int publicationId;

        void Start()
        {
            rosSocket = GetComponent<RosConnector>().RosSocket;
            publicationId = rosSocket.Advertize(topic, "geometry_msgs/Pose");
            MessagePose = new GeometryPose();
        
            
                       
        }

        public void Update()
        {

            Vector3 position = GetComponent<Camera>().transform.position;
            Vector3 eulerAngles = GetComponent<Camera>().transform.rotation.eulerAngles;
            Vector3 eulerAnglesRos = new Vector3 ((360f - eulerAngles.z), ( eulerAngles.x), (360f - eulerAngles.y));
            Vector3 eulerRadiantsRos = eulerAnglesRos * Mathf.PI/180f;
            //Quaternion orientation = Quaternion.Euler(eulerRadiantsRos.x, eulerRadiantsRos.y, eulerRadiantsRos.z);
            //Quaternion orientation = GetComponent<Camera>().transform.rotation;
            //Vector3 position = InputTracking.GetLocalPosition(VRNode.Head);
            //Quaternion orientation = InputTracking.GetLocalRotation(VRNode.Head);

            MessagePose.position.x = position.z;
            MessagePose.position.y = -(position.x);
            MessagePose.position.z = position.y+1.8f;

            MessagePose.orientation.x = eulerRadiantsRos.x;
            MessagePose.orientation.y = eulerRadiantsRos.y;
            MessagePose.orientation.z = eulerRadiantsRos.z;
            MessagePose.orientation.w = 0f;

            //MessagePose.orientation.x = orientation.z;
            //MessagePose.orientation.y = orientation.x;
            //MessagePose.orientation.z = -orientation.y;
            //MessagePose.orientation.w = orientation.w;

            rosSocket.Publish(publicationId, MessagePose);
            

            
            
        }
    }
}