using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
  // Player data elements
  public int Score = 1;
  public string Name;
  public float Radius = 1f; // derivable radius
  public float GrowthRate = 1f; // growth rate: when eating food
  public float SpeedMultiplier = 1f;

  // Player time tracking
  private float survivalTime = 0.0f;
  private float initialisationTime;
  private bool gameOver;

  // UI elements
  public TMP_Text scoreLabel;
  public TMP_Text nameLabel;
  public TMP_Text timeLabel;

  // Start is called before the first frame update
  void Start()
  {
    initialisationTime = Time.timeSinceLevelLoad;
    Radius = Mathf.Sqrt(Score);
    gameOver = false;
    nameLabel.text = Name;
    UpdateGUI();
  }

  // Update is called once per frame
  void Update()
  {
    if (!gameOver)
    {
      survivalTime = Time.timeSinceLevelLoad - initialisationTime;
      UpdateGUI();

      float increment = 0.01f;
      float sizeDifference = transform.localScale.x - Radius;

      if (sizeDifference <= 0)
      {
        transform.localScale += new Vector3(increment, increment, increment);
      }
      else if (sizeDifference > 0 && sizeDifference > increment)
      {
        transform.localScale -= new Vector3(increment, increment, increment);
      }
    }
  }

  // Function to update the GUI
  void UpdateGUI()
  {
    scoreLabel.text = Score.ToString();

    int hours = TimeSpan.FromSeconds(survivalTime).Hours;
    int minutes = TimeSpan.FromSeconds(survivalTime).Minutes;
    int seconds = TimeSpan.FromSeconds(survivalTime).Seconds;

    timeLabel.text = hours.ToString() + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
  }

  // Function to update Score: called by collision events
  public void UpdateScore(int amount)
  {
    Score += amount;
    Radius = Mathf.Sqrt(Score);
    UpdateGUI();
  }

  // Function called on collisions
  void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.tag == "Food")
    {
      int increase = (int)Mathf.Ceil(GrowthRate);
      UpdateScore(increase);
      Destroy(other.gameObject);
      GameManager.RemoveFood(other.gameObject.GetInstanceID());
    }
    else if (other.gameObject.tag == "Enemy")
    {
      if (other.gameObject.GetComponent("Enemy") != null)
      {
        Enemy otherPlayer = other.gameObject.GetComponent<Enemy>();
        int scoreDifference = Score - otherPlayer.getScore();

        if (scoreDifference > 0)
        {
          UpdateScore(scoreDifference);
          Destroy(other.gameObject);
          GameManager.RemoveEnemy(other.gameObject.GetInstanceID());
        }
        else if (scoreDifference < 0)
        {
          gameOver = true;
          Destroy(gameObject);
        }
      }
    }
  }

  // Getter method for score
  public int getScore()
  {
    return Score;
  }

  // Getter method for radius
  public float getRadius()
  {
    return Radius;
  }
}
