using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : IntelligentAgent
{
  // Player statistics tracking
  private float survivalTime = 0.0f;

  // UI elements
  public TMP_Text nameLabel;
  public TMP_Text timeLabel;

  // Start is called before the first frame update
  void Start()
  {
    StartLife();
    this.nameLabel.text = Name;
    UpdateGUI();
	
  }

  // Update is called once per frame
  void Update()
  {
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
}