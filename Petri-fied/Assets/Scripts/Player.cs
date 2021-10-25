using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : IntelligentAgent
{

  public static Player instance;

  // Player statistics tracking
  private float survivalTime = 0.0f;

  // UI elements
  public TMP_Text nameLabel;
  public TMP_Text timeLabel;

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

    // Lastly, update GUI to reflect updated data
    UpdateGUI();
  }

  // Function to update the GUI
  void UpdateGUI()
  {
    int hours = TimeSpan.FromSeconds(this.survivalTime).Hours;
    int minutes = TimeSpan.FromSeconds(this.survivalTime).Minutes;
    int seconds = TimeSpan.FromSeconds(this.survivalTime).Seconds;

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
    GetComponent<LockOnController>().UpdateTargetMaterial(obj);
    this.Target = obj;
  }

  public float getSurvivalTime()
  {
    return this.survivalTime;
  }
}
