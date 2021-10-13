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
        var particleSystem = Instantiate(MagnetEffect, this.transform.position , 
        this.transform.rotation, this.transform);
        particleSystem.transform.localScale = this.transform.localScale;    
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
}
