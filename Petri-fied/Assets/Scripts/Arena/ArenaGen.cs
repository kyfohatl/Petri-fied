using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaGen : MonoBehaviour
{


    public GameObject Skin;
    public Material Mat;
    public float ArenaRad = 50.0f;
    private float basicScale = 50.0f;
    // Start is called before the first frame update
    void Start()
    {
        var model = Instantiate(Skin);
        model.transform.localPosition = this.transform.position;
        model.transform.localScale = new Vector3(ArenaRad-1, ArenaRad-1, 10);
        model.transform.parent = this.transform;
        model.GetComponent<MeshRenderer> ().material = Mat;
        Skin = model;
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(ArenaRad/basicScale,transform.localScale.z , ArenaRad/basicScale);
        //this.transform.localScale.y = ArenaRad/basicScale;    
    }
}
