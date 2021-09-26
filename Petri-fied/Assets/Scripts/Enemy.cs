using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Enemy : MonoBehaviour
{
  // Enemy data elements
  public int Score;
  public string Name;
  public float Radius; // derivable radius
  public float GrowthRate = 1f;// growth rate: when eating food
  public float SpeedMultiplier = 1f;

  // Chosen target.
  public Transform Target;

  // Track player.
  public GameObject Player;

  // Radius of player visibility
  public float LookRadius = 5f;

  // Game manager to load other entities
  public GameManager GameManager;

  // Start is called before the first frame update
  void Start()
  {
    Player = GameObject.FindGameObjectWithTag("Player");
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

    GameObject closestEnemy = GetClosestObject(GameManager.get().getEnemies());

    if (Vector3.Distance(transform.position, Player.transform.position) <= LookRadius && Player.GetComponent<Player>().getScore() < this.getScore())
    {
      // Player has lower score and within 5 radius.
      Target = Player.transform;
    }
    else if (Vector3.Distance(transform.position, closestEnemy.transform.position) <= LookRadius && closestEnemy.GetComponent<Enemy>().getScore() < this.getScore())
    {
      // Another enemy has lower score and within 5 radius.
      Target = closestEnemy.transform;
    }
    else
    {
      // Go for food
      Target = GetClosestObject(GameManager.get().getFood()).transform;
    }
    transform.LookAt(Target);
    transform.position += transform.forward * 2f * Time.deltaTime;
    FaceTarget();
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
          Destroy(gameObject);
          GameManager.RemoveEnemy(gameObject.GetInstanceID());
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

  // Look at target
  void FaceTarget()
  {
    Vector3 direction = (Target.position - transform.position).normalized;
    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
  }

  // Find nearest object in a dictionary
  GameObject GetClosestObject(Dictionary<int, GameObject> objs)
  {
    GameObject tMin = null;
    float minDist = Mathf.Infinity;
    Vector3 currentPos = transform.position;
    foreach (KeyValuePair<int, GameObject> t in objs)
    {
      float dist = Vector3.Distance(t.Value.transform.position, currentPos);
      if (dist < minDist)
      {
        tMin = t.Value;
        minDist = dist;
      }
    }
    return tMin;
  }
}
