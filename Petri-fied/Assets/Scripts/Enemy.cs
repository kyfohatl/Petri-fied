using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    // Enemy data elements
    public int Score;
    public string Name;
    public float Radius; // derivable radius
    public float GrowthRate = 1f;// growth rate: when eating food
    
    // Start is called before the first frame update
    void Start()
    {
        Radius = Mathf.Sqrt(Score);
    }

    // Update is called once per frame
    void Update()
    {
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
    
    // Function to update Score: called by collision events
    public void UpdateScore(int amount)
    {
        Score += amount;
        Radius = Mathf.Sqrt(Score);
    }
    
    // Function called on collisions
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Food")
        {
            int increase = (int)Mathf.Ceil(GrowthRate);
            UpdateScore(increase);
            Destroy(other.gameObject);
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
                }
                else if (scoreDifference < 0)
                {
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
