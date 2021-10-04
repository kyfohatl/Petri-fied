using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaGen : MonoBehaviour
{

    public Material Mat;
    public GameObject Wall;
    public GameObject Flat;
    public float ArenaRad = 50.0f;
    private float basicScale = 50.0f;
    // Start is called before the first frame update
    void Start()
    {
        var Floor = Instantiate(Flat);
        Floor.transform.localPosition = new Vector3(0,10,0) + this.transform.position;
        Floor.transform.parent = this.transform;
        Floor.GetComponent<MeshRenderer> ().material = Mat;
        var Roof = Instantiate(Flat);
        Roof.transform.localPosition = new Vector3(0,-10,0) + this.transform.position;
        Roof.transform.parent = this.transform;
        Roof.GetComponent<MeshRenderer> ().material = Mat;
        
        
        for (int xRot = -180; xRot < 180; xRot += 5){
            var fab = Instantiate(Wall);
            fab.transform.localPosition = new Vector3(basicScale,0,0) + this.transform.position;
            fab.transform.RotateAround(this.transform.position, Vector3.up, xRot);
            fab.transform.parent = this.transform;
            fab.GetComponent<MeshRenderer> ().material = Mat;
            
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(ArenaRad/basicScale,transform.localScale.z , ArenaRad/basicScale);
        //this.transform.localScale.y = ArenaRad/basicScale;    
    }
}
