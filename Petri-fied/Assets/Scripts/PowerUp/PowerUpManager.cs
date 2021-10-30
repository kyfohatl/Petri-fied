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
    PowerUpType = Random.Range(0, 3);
    //Change the visual of the PowerUp pick-up
    switch (PowerUpType)
    {
      case 0:
        GetComponent<Renderer>().material = this.SpeedMat;
        break;
      case 1:
        GetComponent<Renderer>().material = this.MagnetMat;
        break;
      case 2:
        GetComponent<Renderer>().material = this.InvincibleMat;
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

      // Remove lock on target so agent no longer goes for same spot
      other.gameObject.GetComponent<IntelligentAgent>().setTarget(null);

      //Run power Up code
      switch (PowerUpType)
      {
        case 0:
          StartCoroutine(SpeedPowerUp(other));
          break;
        case 1:
          StartCoroutine(FoodMagnetPowerUp(other));
          break;
        case 2:
          StartCoroutine(InvinciblePowerUP(other));
          break;
      }
    }
    else
    {
      return;
    }
  }

  //Changes the player's shader property based on the given propType
  private void SetMat(GameObject actor, string propType)
  {
    //Renderer objRender;
    actor.transform.Find("Avatar").gameObject.GetComponent<MicrobeCore>().ChangeMaterialProperty(propType);
    // Material originalMat = new Material(actor.gameObject.GetComponent<IntelligentAgent>().getStartingMaterial());
    // if (actor.gameObject.tag == "Player")
    // {
    // 	objRender = actor.gameObject.GetComponentInChildren(typeof(Renderer)) as Renderer;
    // }
    // else
    // {
    // 	objRender = actor.gameObject.GetComponent<Renderer>();
    // }

    // Material adaptedMaterial = new Material(newMat);
    // Color newMainColour;
    // if (originalMat.HasProperty("_MainColor"))
    // {
    // 	newMainColour = originalMat.GetColor("_MainColor");
    // }
    // else
    // {
    // 	newMainColour = originalMat.color;
    // }
    // adaptedMaterial.SetColor("_MainColor", newMainColour);
    // objRender.material = adaptedMaterial;
    return;
  }

  // Reverts the shader property type for the given actor back to its original value
  private void RevertMaterial(GameObject actor, string propType)
  {
    if (actor == null)
    {
      return;
    }
    // Reset the target's material
    // if (actor.gameObject.tag == "Player")
    // {
    // 	actor.transform.Find("Avatar").gameObject.GetComponent<Renderer>().material = oldMat;
    // }
    // else
    // {
    // 	actor.gameObject.GetComponent<Renderer>().material = oldMat;
    // }
    actor.transform.Find("Avatar").gameObject.GetComponent<MicrobeCore>().RevertMaterialProperty(propType);
  }


  //Code for Speed Power UP
  IEnumerator SpeedPowerUp(Collider other)
  {

    // Get agent script of other body
    IntelligentAgent actor = other.gameObject.GetComponent<IntelligentAgent>();
    actor.setActivePowers(actor.getActivePowers() + 1);

    //change material
    Material originalMat = actor.getStartingMaterial();
    SetMat(other.gameObject, "speed");

    //Speed power up
    actor.setPowerUpSpeedMultiplier(SpeedPowerUpMult);

    // //Spawn visual effects
    // Transform agentTransform = other.gameObject.transform;
    // var effect1 = Instantiate(SpeedEffect1, agentTransform.position, agentTransform.rotation * Quaternion.Euler(0,180f,0), agentTransform);
    // var effect2 = Instantiate(SpeedEffect2, agentTransform.position, agentTransform.rotation * Quaternion.Euler(0,180f,0), agentTransform);
    // //move an effect forward
    // effect1.transform.localPosition  =  effect1.transform.localPosition + new Vector3(0,0,5);

    //Active powerUp effects
    if (other.gameObject.tag == "Player")
    {
      other.gameObject.transform.Find("SpeedEffect1").gameObject.GetComponent<ParticleSystem>().Play();
    }
    other.gameObject.transform.Find("SpeedEffect2").gameObject.GetComponent<ParticleSystem>().Play();

    //Play sound
    FindObjectOfType<AudioManager>().CreateAndPlay(other.gameObject, "SpeedPowerUP");

    //wait
    yield return new WaitForSeconds(duration);

    // Remove PowerUp Effects if agent isn't destroyed
    if (other != null)
    {
      actor.setPowerUpSpeedMultiplier(BaseSpeedMult);
      actor.setActivePowers(actor.getActivePowers() - 1);
      if (actor.getActivePowers() <= 0)
      {
        RevertMaterial(other.gameObject, "speed");
      }
      // Remove power up from dictionary and destroy
      // Destroy(effect1);
      // Destroy(effect2);
      if (other.gameObject.tag == "Player")
      {
        other.gameObject.transform.Find("SpeedEffect1").gameObject.GetComponent<ParticleSystem>().Stop();
      }
      other.gameObject.transform.Find("SpeedEffect2").gameObject.GetComponent<ParticleSystem>().Stop();
    }

    // Remove power up from dictionary and destroy original power up clone object
    GameManager.RemovePowerUp(gameObject.GetInstanceID());
    Destroy(gameObject);
  }


  IEnumerator FoodMagnetPowerUp(Collider other)
  {
    // Get agent script of other body
    IntelligentAgent actor = other.gameObject.GetComponent<IntelligentAgent>();
    actor.setActivePowers(actor.getActivePowers() + 1);

    // Change material
    Material originalMat = actor.getStartingMaterial();
    SetMat(other.gameObject, "magnet");

    // Instantiate magnet object to be attached to agent
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
    FindObjectOfType<AudioManager>().CreateAndPlay(other.gameObject, "MagnetPowerUP");

    // Now wait until the end of the powerUp
    yield return new WaitForSeconds(duration);

    // Remove PowerUp Effects if agent isnt destroyed
    if (other != null)
    {
      Destroy(magnet); // can be now
      actor.setActivePowers(actor.getActivePowers() - 1);
      if (actor.getActivePowers() <= 0)
      {
        RevertMaterial(other.gameObject, "magnet");
      }
    }

    // Remove power up from dictionary and destroy original power up clone object
    GameManager.RemovePowerUp(gameObject.GetInstanceID());
    Destroy(gameObject);
  }

  IEnumerator InvinciblePowerUP(Collider other)
  {
    // Get renderer and agent script of other body
    Renderer r;
    if (other.gameObject.tag == "Player")
    {
      r = other.gameObject.transform.Find("Avatar").gameObject.GetComponent<Renderer>();
    }
    else
    {
      r = other.gameObject.GetComponent<Renderer>();
    }
    IntelligentAgent actor = other.gameObject.GetComponent<IntelligentAgent>();
    actor.setActivePowers(actor.getActivePowers() + 1);

    //Color objectColor;
    //Color originalColor = r.material.color;
    //objectColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);

    actor.setInvincible(true);
    FindObjectOfType<AudioManager>().CreateAndPlay(other.gameObject, "InvinPowerUP");

    Material originalMat = actor.getStartingMaterial();
    SetMat(other.gameObject, "invincible");
    yield return new WaitForSeconds(duration);

    // if (actor.getActivePowers() == 1)
    // {
    //   RevertMaterial(other.gameObject, originalMat);
    //   yield return new WaitForSeconds(0.1f);
    //   SetMat(other.gameObject, InvincibleMat);
    //   yield return new WaitForSeconds(0.2f);
    //   RevertMaterial(other.gameObject, originalMat);
    //   yield return new WaitForSeconds(0.1f);
    //   SetMat(other.gameObject, InvincibleMat);
    //   yield return new WaitForSeconds(0.15f);
    //   RevertMaterial(other.gameObject, originalMat);
    //   yield return new WaitForSeconds(0.1f);
    //   SetMat(other.gameObject, InvincibleMat);
    //   yield return new WaitForSeconds(0.1f);
    //   RevertMaterial(other.gameObject, originalMat);
    //   yield return new WaitForSeconds(0.05f);
    //   SetMat(other.gameObject, InvincibleMat);
    //   yield return new WaitForSeconds(0.1f);
    //   RevertMaterial(other.gameObject, originalMat);
    //   yield return new WaitForSeconds(0.05f);
    //   SetMat(other.gameObject, InvincibleMat);
    //   yield return new WaitForSeconds(0.05f);
    // }
    // else
    // {
    //   yield return new WaitForSeconds(1f);
    // }

    // Remove PowerUp Effects if agent isnt destroyed
    if (other != null)
    {
      actor.setInvincible(false);
      actor.setActivePowers(actor.getActivePowers() - 1);
      if (actor.getActivePowers() <= 0)
      {
        RevertMaterial(other.gameObject, "invincible");
      }
    }

    // Remove power up from dictionary and destroy original power up clone object
    GameManager.RemovePowerUp(gameObject.GetInstanceID());
    Destroy(gameObject);
  }
}

