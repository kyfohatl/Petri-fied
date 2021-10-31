using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCube : MonoBehaviour
{
	// Rotation rate and euler angle amounts
	public float RotationSpeed = 15f;
	public float xRot = 7;
	public float yRot = 7;
	public float zRot = 7;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
	void Update()
	{
		float xRate = this.xRot * Time.deltaTime * RotationSpeed;
		float yRate = this.yRot * Time.deltaTime * RotationSpeed;
		float zRate = this.zRot * Time.deltaTime * RotationSpeed;
		transform.Rotate(xRate, yRate, zRate);
	}
}
