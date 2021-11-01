using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : IntelligentAgent
{
  // THE MAIN PROTAGONIST
  public static Player instance;

  // UI elements
  public TMP_Text nameLabel;
  public TMP_Text timeLabel;
  public TMP_Text peakScoreLabel;
  public TMP_Text geneticModifierFood;
  public TMP_Text geneticModifierSpeed;
  public TMP_Text geneticModifierLockOn;
  public GameObject powerUpUISpeed;
  public GameObject powerUpUIMagnet;
  public GameObject powerUpUIInvincibility;
  public GameObject powerUpUINullState;

  // Called on start-up of game
  private void Awake()
  {
    instance = this;
    DontDestroyOnLoad(this.gameObject);
    this.nameLabel.text = getName();
  }

  // Start is called before the first frame update
  void Start()
  {
    StartLife();
    this.nameLabel.text = getName();
    UpdateGUI();
  }

  // Update is called once per frame
  void Update()
  {
    if (GameManager.gameOver)
    {
      return;
    }
    this.nameLabel.text = getName();
    this.survivalTime += Time.deltaTime;
    DecayScore();
    UpdateSize();
    UpdateGeneticsUI();
    UpdateActivePowerUpsUI();

    // Lastly, update GUI to reflect updated data
    UpdateGUI();
  }

  void UpdateActivePowerUpsUI()
  {
    // Show null state if no powers
    if (!this.isInvincible() && !this.isSpeed() && !this.isMagnet())
    {
      powerUpUINullState.SetActive(true);
    }
    else
    {
      powerUpUINullState.SetActive(false);
    }
    Debug.Log("HAS SPEED BOOST");
    Debug.Log(this.isSpeed());
    Debug.Log("HAS MAGNET");
    Debug.Log(this.isMagnet());
    Debug.Log("HAS INVINC");
    Debug.Log(this.isInvincible());

    // Choose which powerup ui elements should be visible.
    if (powerUpUISpeed)
    {
      if (this.isSpeed())
      {
        powerUpUISpeed.SetActive(true);
      }
      else
      {
        powerUpUISpeed.SetActive(false);
      }
    }
    if (powerUpUIMagnet)
    {
      if (this.isMagnet())
      {
        powerUpUIMagnet.SetActive(true);
      }
      else
      {
        powerUpUIMagnet.SetActive(false);
      }
    }
    if (powerUpUIInvincibility)
    {
      if (this.isInvincible())
      {
        powerUpUIInvincibility.SetActive(true);
      }
      else
      {
        powerUpUIInvincibility.SetActive(false);
      }
    }
  }

  void UpdateGeneticsUI()
  {
    if (geneticModifierFood && geneticModifierSpeed && geneticModifierLockOn)
    {
      geneticModifierFood.text = Math.Round(this.FoodGrowthMultiplier, 2).ToString();
      geneticModifierSpeed.text = Math.Round(this.SpeedMultiplier, 2).ToString();
      geneticModifierLockOn.text = Math.Round(this.LockOnRadiusMultiplier, 2).ToString();
    }
  }

  // Function to update the GUI
  void UpdateGUI()
  {
    int hours = TimeSpan.FromSeconds(this.survivalTime).Hours;
    int minutes = TimeSpan.FromSeconds(this.survivalTime).Minutes;
    int seconds = TimeSpan.FromSeconds(this.survivalTime).Seconds;

    this.peakScoreLabel.text = peakScore.ToString();
    this.timeLabel.text = hours.ToString() + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
  }


  // Override function to update the player radius and notify the game event handler
  public override void UpdateRadius()
  {
    base.UpdateRadius();
    GameEvents.instance.PlayerRadiusChange(Radius);
  }

  // Override function to set the player's target and change target's material properties
  public override void setTarget(GameObject obj)
  {
    if (obj != null)
    {
      FindObjectOfType<AudioManager>().CreateAndPlay(this.gameObject, "LockOn");
      float dist = Vector3.Distance(obj.gameObject.transform.position, transform.position);
      float travelTime = dist / (getSpeedMultiplier() * getPowerUpSpeedMultiplier() / transform.localScale.x);
      Debug.Log("Distance to target: " + dist + ", expected travel time: " + travelTime + " seconds");

      string targetTag = obj.gameObject.tag;
      if (targetTag == "Enemy")
      {
        Debug.Log("Locked-onto enemy player: " + obj.GetComponent<IntelligentAgent>().getName());
      }
      else if (targetTag == "Food" || targetTag == "SuperFood")
      {
        Debug.Log("Locked-onto " + targetTag);
      }
      else if (targetTag == "PowerUp")
      {
        Debug.Log("Locked-onto Power-Up");
      }
    }
    else
    {
      // Target is being reset to null
      FindObjectOfType<AudioManager>().CreateAndPlay(this.gameObject, "FailedLockOn");
    }

    // Finally check if previous target needs material adjustment and set the new target
    GetComponent<LockOnController>().UpdateTargetMaterial(obj);
    this.Target = obj;
  }

  // Getter function for player survivial time
  public float getSurvivalTime()
  {
    return this.survivalTime;
  }
}
