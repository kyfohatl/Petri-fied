using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodMagnetPowerUP : MonoBehaviour
{
    public float MagnetStrength = 1f;
	public float MagnetScale = 3f;
    public GameObject MagnetEffect;
    private GameObject pSystem;
    // Start is called before the first frame update
    void Start()
    {
		// Determine initial local scale
		if (this.transform.parent.tag == "Enemy")
		{
			this.transform.position = this.transform.parent.Find("Avatar").position;
			this.transform.localScale = this.MagnetScale * this.transform.parent.Find("Avatar").localScale;
		}
		else
		{
			this.transform.localScale = this.MagnetScale * new Vector3(1f, 1f, 1f);
		}
		// Instantiate the particle effect
        pSystem = Instantiate(MagnetEffect, this.transform.position, this.transform.rotation, this.transform);
		pSystem.transform.localScale = this.transform.localScale;
        var sh = pSystem.GetComponent<ParticleSystem>().shape;
        sh.shapeType = ParticleSystemShapeType.Mesh;
        sh.mesh =  createNewMesh();
    }
	
	// Update is called every frame (needed for enemy)
	void Update()
	{
		if (this.transform.parent.tag == "Enemy")
		{
			this.transform.position = this.transform.parent.Find("Avatar").position;
			this.transform.localScale = this.MagnetScale * this.transform.parent.Find("Avatar").localScale;
		}
	}

	// If hits food, pull food to centre
	void OnTriggerEnter(Collider FoodHit)
	{
		if (FoodHit.gameObject.tag == "Food" || FoodHit.gameObject.tag == "SuperFood")
		{
			Vector3 direction = (this.transform.position - FoodHit.gameObject.transform.position).normalized;
			FoodHit.gameObject.transform.Translate(direction * MagnetStrength * Time.deltaTime, Space.World);
		}
		else
		{
			return;
		}
	}
	
    // If hits food, pull food to centre
    void OnTriggerStay(Collider FoodHit)
	{
        if (FoodHit.gameObject.tag == "Food" || FoodHit.gameObject.tag == "SuperFood")
		{
			Vector3 offset = FoodHit.gameObject.transform.position - this.transform.position;
			float sqrdLength = offset.sqrMagnitude;
			float closenessFraction = this.transform.localScale.x * this.transform.localScale.x / sqrdLength;
			float strength = closenessFraction * this.MagnetStrength;
            Vector3 direction = -offset.normalized; // reverse the offset vector
            FoodHit.gameObject.transform.Translate(direction * strength * Time.deltaTime, Space.World);
        }
		else
		{
            return;
        }
    }

    private Mesh createNewMesh(){

        Mesh meshOld = GetComponent<MeshFilter>().mesh;
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] normals = mesh.normals;
        for(int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        mesh.normals = normals;
        
        for (int i =0; i < mesh.subMeshCount; i++){
            int[] tris = mesh.GetTriangles(i);
            for (int j = 0; j < tris.Length; j+= 3){
                int temp = tris[j];
                tris[j] = tris[j + 1];
                tris[j + 1] = temp;
            }
            mesh.SetTriangles(tris,i);
        }


        GetComponent<MeshFilter>().mesh = meshOld;
        return mesh;
    }
}
