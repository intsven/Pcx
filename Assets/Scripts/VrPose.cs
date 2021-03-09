using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;




public class VrPose : MonoBehaviour
{
    private const string logPrefix = "InputTrackerChecker: ";

    [SerializeField] UnityEngine.XR.XRNode m_VRNode = UnityEngine.XR.XRNode.Head;

    private void Start()
    {
        StartCoroutine(EndOfFrameUpdate());
        
    }

    private void Update()
    {
        LogRotation("Update");
    }

    private void LateUpdate()
    {
        LogRotation("LateUpdate");
    }

    private IEnumerator EndOfFrameUpdate()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            LogRotation("EndOfFrame");
        }
    }

    private void LogRotation(string id)
    {
        var quaternion = UnityEngine.XR.InputTracking.GetLocalRotation(m_VRNode);
        var euler = quaternion.eulerAngles;
        Debug.Log(string.Format("{0} {1}, ({2}) Quaternion {3} Euler {4}", logPrefix, id, m_VRNode, quaternion.ToString("F2"), euler.ToString("F2")));
        var quaterions = GetComponent<Camera>().transform.rotation;
        //float x = quaternion.x - quaterions.x;
        //float y = quaternion.y - quaterions.y;
        //float z = quaternion.z - quaterions.z;
        
        

    }
}