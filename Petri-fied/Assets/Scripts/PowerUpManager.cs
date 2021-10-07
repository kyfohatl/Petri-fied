using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{

    public float SpeedPowerUpMult = 5.0f;
    public float duration = 5f;
    private float BaseSpeedMult = 1f;
    public GameObject SpeedEffect1;//Will call these effect from folder in final product
    public GameObject SpeedEffect2;
    // Start is called before the first frame update
    void Start()
    {  
        //pick random effect for the powerup 
    }

    // Update is called once per frame
    void Update()
    {
                
        
    }

    void OnTriggerEnter(Collider other){
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Player"){
            //Remove PowerUp object
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            StartCoroutine( SpeedPowerUp(other));
        }else{
            return;
        }
    }

    IEnumerator SpeedPowerUp(Collider other){


        
        IntelligentAgent actor = other.gameObject.GetComponent<IntelligentAgent>();;


        //Speed power up
        actor.setPowerUpSpeedMultiplier(SpeedPowerUpMult);

        //Spawn visual effects
        var effect1 = Instantiate(SpeedEffect1, other.gameObject.transform.position , 
        other.gameObject.transform.rotation * Quaternion.Euler(0,180f,0), other.gameObject.transform);
        var effect2 = Instantiate(SpeedEffect2, other.gameObject.transform.position, 
        other.gameObject.transform.rotation * Quaternion.Euler(0,180f,0), other.gameObject.transform);
        //move an effect forward
        effect1.transform.localPosition  =  effect1.transform.localPosition + new Vector3(0,0,5);
        
        //wait
        yield return new WaitForSeconds(duration);

        //Remove PowerUp Effects
        actor.setPowerUpSpeedMultiplier(BaseSpeedMult);
        Destroy(effect1);
        Destroy(effect2);
        GameManager.RemovePowerUp(gameObject.GetInstanceID());
        Destroy(gameObject);
    }
}
