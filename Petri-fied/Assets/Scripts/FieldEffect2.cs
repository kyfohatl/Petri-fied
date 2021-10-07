using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldEffect2 : MonoBehaviour
{
    public Material shaderMat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        shaderMat.SetVector("_Position", transform.position);
        
    }
}
