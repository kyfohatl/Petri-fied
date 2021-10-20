using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaSize : MonoBehaviour
{
    public float ArenaRadius = 150.0f;
    public float ArenaHeight = 100.0f;
    private float basicScale = 1.0f;
    private float HeightRefactor = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		// Temp code to deal with pro builder still existing in some scenes causing error. Can be removed after pro builder stuff is removed
		var mesh = this.GetComponent<MeshFilter>().mesh;
		
		if (mesh.name == "Cylinder")
		{
			transform.localScale = new Vector3(this.ArenaRadius, this.ArenaRadius, this.ArenaHeight * this.HeightRefactor) / this.basicScale;
		}
    }
}
