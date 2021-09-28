using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelligentAgent : MonoBehaviour
{
  // Agent data elements
  public string Name = "JoeMama-420";
  public int Score = 1;
  public float Radius = 1f;

  // Agent genetic modifiers
  public float FoodGrowthMultiplier;
  public float SpeedMultiplier;
  public float ScoreDecayRate; // percentage of score loss per decay

  // Agent statistics
  private int peakScore;
  private float initialisationTime;

  // Default Constructor
  public IntelligentAgent(string givenName)
  {
    initialisationTime = Time.timeSinceLevelLoad;
    Name = givenName;
    GenerateRandomGenetics();
  }

  // Function to generate random starting genetics
  public void GenerateRandomGenetics()
  {
    this.FoodGrowthMultiplier = UnityEngine.Random.Range(1f, 3f);
    this.SpeedMultiplier = UnityEngine.Random.Range(1f, 3f);
    this.ScoreDecayRate = UnityEngine.Random.Range(0.01f, 0.05f);
  }

  // Function called on collisions
  void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.tag == "Food")
    {
      int increase = (int)Mathf.Round(this.FoodGrowthMultiplier);
      UpdateScore(increase);
      Destroy(other.gameObject);
      GameManager.RemoveFood(other.gameObject.GetInstanceID());
    }
    else if (other.gameObject.tag == "Enemy")
    {
      if (other.gameObject.GetComponent("Enemy") != null)
      {
        Enemy otherPlayer = other.gameObject.GetComponent<Enemy>();
        int scoreDifference = this.Score - otherPlayer.getScore();

        if (scoreDifference > 0)
        {
          UpdateScore(otherPlayer.getScore());
          Destroy(other.gameObject);
          GameManager.RemoveEnemy(other.gameObject.GetInstanceID());
        }
        else if (scoreDifference < 0)
        {
          Destroy(gameObject);
          GameManager.RemoveEnemy(gameObject.GetInstanceID());
        }
      }
    }
  }

  // Function to update radius
  public virtual void UpdateRadius()
  {
    this.Radius = (float)Mathf.Pow(this.Score, 1f / 3f);
  }

  // Function to update Score: called by collision events
  public void UpdateScore(int amount)
  {
    this.Score += amount;
    UpdateRadius();
  }

  // Function to update size
  public void UpdateSize()
  {
    float sizeChangeSpeed = 5.0f;
    float sizeDifference = this.Radius - transform.localScale.x;

    float X = transform.localScale.x + sizeDifference;
    float Y = transform.localScale.y + sizeDifference;
    float Z = transform.localScale.z + sizeDifference;
    Vector3 newScale = new Vector3(X, Y, Z);

    transform.localScale = Vector3.Lerp(transform.localScale, newScale, sizeChangeSpeed * Time.deltaTime);
  }

  // Function to implement score decay
  public void DecayScore()
  {
    int reduction = (int)Mathf.Floor(this.ScoreDecayRate * this.Score);
    if (this.Score > 1)
    {
      // Minimum reduction per decay is 1, take min between calculated neg reduction and -1
      reduction = (int)Mathf.Min(-reduction, -1);
      UpdateScore(reduction);
    }
  }

  // Getter method for name
  public string getName()
  {
    return this.Name;
  }

  // Getter method for score
  public int getScore()
  {
    return this.Score;
  }

  // Getter method for radius
  public float getRadius()
  {
    return this.Radius;
  }

  // Getter method for speed multiplier
  public float getSpeedMultiplier()
  {
    return this.SpeedMultiplier;
  }

  // Getter method for initialisation time
  public float getInitialisationTime()
  {
    return this.initialisationTime;
  }

  // Getter method for peak score
  public float getPeakScore()
  {
    return this.peakScore;
  }
}
