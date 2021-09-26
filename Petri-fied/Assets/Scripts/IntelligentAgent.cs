using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelligentAgent : MonoBehaviour
{
    // Agent metadata elements
    public string Name;
    public int Score = 1;
    public float Radius = 1f;
    
    // Agent genetic modifiers
    public float FoodGrowthMultiplier;
    public float SpeedMultiplier;
    public float ScoreDecayMultiplier;
    
    // Agent statistics tracking
    private float initialisationTime;
    private int peakScore;
    
    // Second constructor with string argument
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
        this.ScoreDecayMultiplier = UnityEngine.Random.Range(0.5f, 3f);
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
                    UpdateScore(scoreDifference);
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
        float increment = 0.01f;
        float sizeDifference = transform.localScale.x - this.Radius;
        
        if (sizeDifference < 0)
        {
            transform.localScale += new Vector3(increment, increment, increment);
        }
        else if (sizeDifference > 0 && sizeDifference > increment)
        {
            transform.localScale -= new Vector3(increment, increment, increment);
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
    
    // Getter method for initialisation time
    public float getInitialisationTime()
    {
        return this.initialisationTime;
    }
    
    // Getter method for initialisation time
    public float getPeakScore()
    {
        return this.peakScore;
    }
}
