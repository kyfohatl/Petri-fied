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
    private ParticleSystem ps;
    public Material SpeedMat;
    public Material MagnetMat;
    public Material InvincibleMat;
    // Start is called before the first frame update
    void Start()
    {  
        //pick random effect for the powerup
        // 0 is Speed PowerUP
        // 1 is Food Magnet
        // 2 is Invincible
        PowerUpType = Random.Range(0,3);
        ps = GetComponent<ParticleSystem>();
        //Change the visual of the PowerUp pick-up
        switch(PowerUpType){
            case 0:
                setColor(Color.blue); //Temp Color
                break;
            case 1:
                setColor(Color.green); //Temp Color
                break;
            case 2:
                setColor(Color.yellow); //Temp Color
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {
                
        
    }

    void OnTriggerEnter(Collider other)
	{
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Player")
		{
            //Remove PowerUp object (visually)
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
            Destroy(ps);
			
			// Remove lock on target so agent no longer goes for same spot
			other.gameObject.GetComponent<IntelligentAgent>().setTarget(null);
			
            //Run power Up code
            switch(PowerUpType)
            {
                case 0:
                    StartCoroutine( SpeedPowerUp(other));
                    break;
                case 1:
                    StartCoroutine( FoodMagnetPowerUp(other));
                    break;
                case 2:
                    StartCoroutine( InvinciblePowerUP(other));
                    break;
            }
        }
		else
		{
            return;
        }
    }

    private void setColor(Color setTo){
        //
        //GetComponent<Renderer>().material.color = setTo;
        var main = ps.main;
        main.startColor = setTo;

    }

    //Changes the player's material and return the old material 
    private void SetMat(GameObject player, Material newMat){
        Renderer objRender;
        Material originalMat = new Material(player.gameObject.GetComponent<IntelligentAgent>().getStartingMaterial());
        if(player.gameObject.tag == "Player"){
            objRender = player.gameObject.GetComponentInChildren(typeof(Renderer)) as Renderer;
        }else{
            objRender = player.gameObject.GetComponent<Renderer>();
        }

        

    	Material adaptedMaterial = new Material(newMat);
        Color newMainColour;
		if (originalMat.HasProperty("_MainColor"))
		{
			newMainColour = originalMat.GetColor("_MainColor");
		}
		else
		{
			newMainColour = originalMat.color;
		}
		adaptedMaterial.SetColor("_MainColor", newMainColour);
        objRender.material = adaptedMaterial;
        return;
    }

    private void RevertMaterial(GameObject player, Material oldMat)
	{
        if(player == null){
            return;
        }
		// Reset the target's material
        if(player.gameObject.tag == "Player"){
            player.transform.Find("Avatar").gameObject.GetComponent<Renderer>().material = oldMat;
        }else{
            player.gameObject.GetComponent<Renderer>().material = oldMat;
        }
	}


    //Code for Speed Power UP
    IEnumerator SpeedPowerUp(Collider other)
	{
        
		// Get agent script of other body
		IntelligentAgent actor = other.gameObject.GetComponent<IntelligentAgent>();
        actor.setActivePowers(actor.getActivePowers() + 1);

        //change material
        Material originalMat = actor.getStartingMaterial();
        SetMat(other.gameObject, SpeedMat);

        //Speed power up
        actor.setPowerUpSpeedMultiplier(SpeedPowerUpMult);

        // //Spawn visual effects
		// Transform agentTransform = other.gameObject.transform;
		// var effect1 = Instantiate(SpeedEffect1, agentTransform.position, agentTransform.rotation * Quaternion.Euler(0,180f,0), agentTransform);
        // var effect2 = Instantiate(SpeedEffect2, agentTransform.position, agentTransform.rotation * Quaternion.Euler(0,180f,0), agentTransform);
		// //move an effect forward
        // effect1.transform.localPosition  =  effect1.transform.localPosition + new Vector3(0,0,5);

        //Active powerUp effects
        if(other.gameObject.tag == "Player"){
            other.gameObject.transform.Find("SpeedEffect1").gameObject.GetComponent<ParticleSystem>().Play();
        }        
        other.gameObject.transform.Find("SpeedEffect2").gameObject.GetComponent<ParticleSystem>().Play();

        //Play sound
        FindObjectOfType<AudioManager>().CreateAndPlay(other.gameObject,"SpeedPowerUP");
        
        //wait
        yield return new WaitForSeconds(duration);

        //Remove PowerUp Effects
        if (other == null)
		{
			// Actor was deleted between yield coroutine execution
			GameManager.RemovePowerUp(gameObject.GetInstanceID());
            Destroy(gameObject);
        }
		else
		{
			actor.setPowerUpSpeedMultiplier(BaseSpeedMult);
			actor.setActivePowers(actor.getActivePowers() - 1);
			if(actor.getActivePowers() <= 0)
			{
				RevertMaterial(other.gameObject, originalMat);
			}
			// Remove power up from dictionary and destroy
			// Destroy(effect1);
			// Destroy(effect2);
            if(other.gameObject.tag == "Player"){
                other.gameObject.transform.Find("SpeedEffect1").gameObject.GetComponent<ParticleSystem>().Stop();
            }
            other.gameObject.transform.Find("SpeedEffect2").gameObject.GetComponent<ParticleSystem>().Stop();


			GameManager.RemovePowerUp(gameObject.GetInstanceID());
			Destroy(gameObject);
		}
    }


     IEnumerator FoodMagnetPowerUp(Collider other)
	 {
		// Get agent script of other body
		IntelligentAgent actor = other.gameObject.GetComponent<IntelligentAgent>();
        actor.setActivePowers(actor.getActivePowers() + 1);

        // Change material
        Material originalMat = actor.getStartingMaterial();
        SetMat(other.gameObject, MagnetMat);

        // Create magnet
		GameObject magnet = null;
		if (other.gameObject.tag == "Player")
		{
			magnet = Instantiate(FoodMagnet, other.gameObject.transform);
		}
		else
		{
			magnet = Instantiate(FoodMagnet, other.gameObject.transform.parent);
		}
		magnet.GetComponent<FoodMagnetPowerUP>().MagnetScale = FoodMagnetScale;
        magnet.GetComponent<FoodMagnetPowerUP>().MagnetStrength = FoodMagnetSpeed;
		FindObjectOfType<AudioManager>().CreateAndPlay(other.gameObject,"MagnetPowerUP");

        yield return new WaitForSeconds(duration);
		
		// Remove PowerUp Effects
		if (other == null)
		{
			GameManager.RemovePowerUp(gameObject.GetInstanceID());
		}
		else
		{
			actor.setActivePowers(actor.getActivePowers() - 1);
			if(actor.getActivePowers() <= 0)
			{
				RevertMaterial(other.gameObject, originalMat);
			}
			// Remove power up from dictionary and destroy
			GameManager.RemovePowerUp(gameObject.GetInstanceID());
			Destroy(magnet);
			Destroy(gameObject);
		}
     }

    IEnumerator InvinciblePowerUP(Collider other)
	{
		// Get renderer and agent script of other body
		Renderer r;
		if (other.gameObject.tag == "Player")
		{
			r = other.gameObject.transform.GetChild(0).gameObject.GetComponent<Renderer>();
		}
		else
		{
			r = other.gameObject.GetComponent<Renderer>();
		}
		IntelligentAgent actor = other.gameObject.GetComponentInParent<IntelligentAgent>();
        actor.setActivePowers(actor.getActivePowers() + 1);

        //Color objectColor;
        //Color originalColor = r.material.color;
        //objectColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);

        actor.setInvincible(true);
		FindObjectOfType<AudioManager>().CreateAndPlay(other.gameObject,"InvinPowerUP");

        Material originalMat = actor.getStartingMaterial();
        SetMat(other.gameObject, InvincibleMat);
        yield return new WaitForSeconds(duration);

        if(actor.getActivePowers() == 1){
            RevertMaterial(other.gameObject, originalMat);
            yield return new WaitForSeconds(0.1f);
            SetMat(other.gameObject, InvincibleMat);
            yield return new WaitForSeconds(0.2f);
            RevertMaterial(other.gameObject, originalMat);
            yield return new WaitForSeconds(0.1f);
            SetMat(other.gameObject, InvincibleMat);
            yield return new WaitForSeconds(0.15f);
            RevertMaterial(other.gameObject, originalMat);
            yield return new WaitForSeconds(0.1f);
            SetMat(other.gameObject, InvincibleMat);
            yield return new WaitForSeconds(0.1f);
            RevertMaterial(other.gameObject, originalMat);
            yield return new WaitForSeconds(0.05f);
            SetMat(other.gameObject, InvincibleMat);
            yield return new WaitForSeconds(0.1f);
            RevertMaterial(other.gameObject, originalMat);
            yield return new WaitForSeconds(0.05f);
            SetMat(other.gameObject, InvincibleMat);
            yield return new WaitForSeconds(0.05f);
        }else{
            yield return new WaitForSeconds(1f);
        }
		
		// Remove PowerUp Effects
		if (other == null)
		{
			GameManager.RemovePowerUp(gameObject.GetInstanceID());
		}
		else
		{
			actor.setInvincible(false);
			actor.setActivePowers(actor.getActivePowers() - 1);
			if (actor.getActivePowers() <= 0)
			{
				RevertMaterial(other.gameObject, originalMat);
			}
			
			// Remove power up from dictionary and destroy
			GameManager.RemovePowerUp(gameObject.GetInstanceID());
			Destroy(gameObject);
		}
    }




}

