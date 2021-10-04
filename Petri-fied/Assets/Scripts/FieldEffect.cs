using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldEffect : MonoBehaviour
{

    public string tagToHit;
    public Material shaderMat;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.GetComponent<ParticleSystem>().IsAlive())
        {
           this.GetComponent<Collider>().isTrigger = true;
        }
        
    }

    private void OnTriggerEnter(Collider col)
    {
        
        if (col.gameObject.tag == tagToHit)
        {
            shaderMat.SetVector("_Position", col.gameObject.transform.position);
            GetComponent<ParticleSystem>().Play();
            //Debug.Log("Hit");
            this.GetComponent<Collider>().isTrigger = false;
            
        }
        
        
    }

}
