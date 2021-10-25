using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Siccity.GLTFUtility;

public class GLTFTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject result = Importer.LoadFromFile("microphone");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
