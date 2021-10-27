using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodMagnetPowerUP : MonoBehaviour
{
    public float MagnetStrength;
    public GameObject MagnetEffect;
    private GameObject pSystem;
    // Start is called before the first frame update
    void Start()
    {
        pSystem = Instantiate(MagnetEffect, this.transform.position, this.transform.rotation, this.transform);
        pSystem.transform.localScale = this.transform.localScale;    
        var sh = pSystem.GetComponent<ParticleSystem>().shape;
        sh.shapeType = ParticleSystemShapeType.Mesh;
        sh.mesh =  createNewMesh();
    }

    // Update is called once per frame
    void Update()
    {
        float effectScale = transform.localScale.x * transform.parent.localScale.y;
        pSystem.transform.localScale = new Vector3(effectScale, effectScale, effectScale); 
    }

    //If hits food, pull food to centre
    void OnTriggerStay(Collider FoodHit)
	{
        if (FoodHit.gameObject.tag == "Food")
		{
            Vector3 direction = (transform.parent.transform.position - FoodHit.gameObject.transform.position).normalized;
            FoodHit.gameObject.transform.Translate(direction * MagnetStrength * Time.deltaTime, Space.World);
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
