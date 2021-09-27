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
  public TMP_Text scoreLabel;
  public TMP_Text nameLabel;
  public TMP_Text timeLabel;
  
  // Function to call parent contructor
  public Player(string givenName) : base(givenName)
  {
    Debug.Log("Player: " + Name + " has been generated into the game.");
  }
  
  // Start is called before the first frame update
  void Start()
  {
    nameLabel.text = Name;
    UpdateGUI();
    InvokeRepeating("DecayScore", 10.0f, 10.0f); // first call: 10s, repeats: 10s
  }
  
  // Update is called once per frame
  void Update()
  {
    this.survivalTime = Time.timeSinceLevelLoad - base.getInitialisationTime();
    UpdateSize();
    UpdateGUI();
  }
  
  // Function to update the GUI
  void UpdateGUI()
  {
    this.scoreLabel.text = Score.ToString();
    
    int hours = TimeSpan.FromSeconds(this.survivalTime).Hours;
    int minutes = TimeSpan.FromSeconds(this.survivalTime).Minutes;
    int seconds = TimeSpan.FromSeconds(this.survivalTime).Seconds;
    
    this.timeLabel.text = hours.ToString() + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
  }
}
