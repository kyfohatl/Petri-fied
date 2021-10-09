using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{

    public float SpeedPowerUpMult = 5.0f;
    public float duration = 5f;
    private float BaseSpeedMult = 1f;
    public float FoodMagnetScale = 5f;
    public float FoodMagnetSpeed = 0.5f;
    public GameObject SpeedEffect1;//Will call these effect from folder in final product
    public GameObject SpeedEffect2;
    public GameObject FoodMagnet;
    private int PowerUpType;
    // Start is called before the first frame update
    void Start()
    {  
        //pick random effect for the powerup
        // 0 is Speed PowerUP
        // 1 is Food Magnet
        PowerUpType = Random.Range(0,2);
        //Change the visual of the PowerUp pick-up
        switch(PowerUpType){
            case 0:
                GetComponent<Renderer>().material.color = Color.red; //Temp Color
                break;
            case 1:
                GetComponent<Renderer>().material.color = Color.green; //Temp Color
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
                
        
    }

    void OnTriggerEnter(Collider other){
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Player"){
            //Remove PowerUp object (visually)
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;

            //Run power Up code
            switch(PowerUpType)
            {
                case 0:
                    StartCoroutine( SpeedPowerUp(other));
                    break;
                case 1:
                    StartCoroutine( FoodMagnetPowerUp(other));
                    break;
            }
        }else{
            return;
        }
    }


    //Code for Speed Power UP
    IEnumerator SpeedPowerUp(Collider other){


        
        IntelligentAgent actor = other.gameObject.GetComponent<IntelligentAgent>();


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


     IEnumerator FoodMagnetPowerUp(Collider other){

         //create magnet
        var magnet = Instantiate(FoodMagnet);
        magnet.transform.localPosition = other.gameObject.transform.position;
        magnet.transform.localScale = new Vector3(FoodMagnetScale,FoodMagnetScale,FoodMagnetScale);
        magnet.transform.parent = other.transform;

        magnet.GetComponent<FoodMagnetPowerUP>().MagnetStrength = FoodMagnetSpeed;

        yield return new WaitForSeconds(duration);


        Destroy(magnet);
        Destroy(gameObject);
     }
}
