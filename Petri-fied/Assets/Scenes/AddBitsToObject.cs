using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddBitsToObject : MonoBehaviour {

    // External parameters/variables

    /*
        a few notes:
        -We will need to update this code for when the object changes size
        -Currently we need to add all materials and prefabs in unity
        -It is easy change the colour (to not use Materials) but no idea for the shapes

    */


    public GameObject rodTemplate;
    public GameObject headTemplate;

    public Material Mat1;
    public Material Mat2;

    private float stepCountdown;
    

    // Use this for initialization
    void Start () {
        Generate();


	}
	
	// Update is called once per frame
	void Update () {

    }

    // Method to automatically generate swarm of enemies based on the set public attributes
    private void Generate()
    {
        /*
        
        //some functions that we might want to use

        //rotate an object around itself
        fab.transform.localRotation = Quaternion.Euler(0, 0, 90);
        
        //This set a new size of an object 
        fab.transform.localScale = new Vector3(0.1f,0.3f,0.1f);

        //this line controls the mat
        fab.GetComponent<MeshRenderer> ().material = MaterialHere;

        //to set colour (instead of materal)
        fab.GetComponent<Renderer>().material.color = Color.green;
        
        */
        

        for (int xRot = 0; xRot < 180; xRot += 40)
        {
            for (int yRot = -180; yRot < 180; yRot += 40)
            {
                var fabRod = Instantiate(rodTemplate);
                fabRod.transform.localPosition = new Vector3(0,this.transform.localScale.y/2,0);
                fabRod.transform.RotateAround(this.transform.position, Vector3.forward, xRot);
                fabRod.transform.RotateAround(this.transform.position, Vector3.up, yRot);
                fabRod.transform.parent = this.transform;
                fabRod.GetComponent<MeshRenderer> ().material = Mat2;


                var fabHead = Instantiate(headTemplate);
                fabHead.transform.localPosition = new Vector3(0,this.transform.localScale.y/2 + fabRod.transform.localScale.y,0);
                fabHead.transform.RotateAround(this.transform.position, Vector3.forward, xRot);
                fabHead.transform.RotateAround(this.transform.position, Vector3.up, yRot);
                fabHead.transform.parent = this.transform;
                //fabHead.GetComponent<MeshRenderer> ().material = Mat3;
                fabHead.GetComponent<Renderer>().material.color = Color.green;

                
            }
            
        }
        
        
    }

}
