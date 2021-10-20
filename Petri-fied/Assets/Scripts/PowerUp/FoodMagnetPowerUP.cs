using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodMagnetPowerUP : MonoBehaviour
{
    public float MagnetStrength;
    public GameObject MagnetEffect;
    // Start is called before the first frame update
    void Start()
    {
        var particleSystem = Instantiate(MagnetEffect, this.transform.position, this.transform.rotation, this.transform);
        particleSystem.transform.localScale = this.transform.localScale;    
        var sh = particleSystem.GetComponent<ParticleSystem>().shape;
        sh.shapeType = ParticleSystemShapeType.Mesh;
        sh.mesh =  createNewMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //If hits food, pull food to centre
    void OnTriggerStay(Collider FoodHit)
	{
        if (FoodHit.gameObject.tag == "Food")
		{
            Vector3 direction = (transform.position - FoodHit.gameObject.transform.position).normalized;
            FoodHit.gameObject.transform.Translate(direction * MagnetStrength * Time.deltaTime);
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