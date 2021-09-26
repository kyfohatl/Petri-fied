using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Enemy : IntelligentAgent
{
    // Target and determination delay
    public Transform Target;
    private float delayTimer;
    private float deltaDetermine = 1.0f; // delay between targeting
    
    // Track player
    public GameObject Player;
    
    // Radius of player visibility
    public float LookRadius;
    public float LookRadiusMultiplier = 5f;
    
    // Game manager to load other entities
    public GameManager GameManager;
    
    // Function to call parent contructor
    public Enemy(string givenName) : base(givenName)
    {
        Debug.Log("Enemy: " + Name + " has been generated into the game.");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        GenerateRandomName();
        Player = GameObject.FindGameObjectWithTag("Player");
        LookRadius = LookRadiusMultiplier * Radius;
        DetermineTarget();
        delayTimer = deltaDetermine;
    }
    
    // Update is called once per frame
    void Update()
    {
        UpdateSize();
        this.delayTimer -= Time.deltaTime;
        
        if (this.delayTimer < 0f)
        {
            this.delayTimer = this.deltaDetermine;
            DetermineTarget();
        }
        
        if (Target != null)
        {
            FaceTarget();
            transform.position += transform.forward * SpeedMultiplier * Time.deltaTime;
        }
    }
    
    // Override Function to update radius and also look radius
    public override void UpdateRadius()
    {
        base.UpdateRadius();
        this.LookRadius = LookRadiusMultiplier * Radius;
    }
    
    // Function to generate a random name
    private void GenerateRandomName()
    {
        string randomString = "";
        int digitCount = 3;
        
        randomString += (char)UnityEngine.Random.Range('A', 'Z');
        for (int i = 0; i < digitCount; i++)
        {
            randomString += (char)UnityEngine.Random.Range('0', '9');
        }
        randomString += "-" + (char)UnityEngine.Random.Range('A', 'Z');
        
        this.Name = randomString;
    }
    
    // Function to determine target
    public void DetermineTarget()
    {
        GameObject closestEnemy = null;
        GameObject closestFood = null;
        
        float playerDistance = Vector3.Distance(transform.position, Player.transform.position);
        int playerScore = Player.GetComponent<Player>().getScore();
        
        // Prioritise player first
        if (playerDistance <= LookRadius && playerScore < this.getScore())
        {
            // Player has lower score and within 5 radius.
            Target = Player.transform;
            return;
        }
        
        // Secondly, other enemies
        closestEnemy = GetClosestObject(GameManager.get().getEnemies());
        if (closestEnemy != null)
        {
            float enemyDistance = Vector3.Distance(transform.position, closestEnemy.transform.position);
            int enemyScore = closestEnemy.GetComponent<Enemy>().getScore();
            
            if (enemyDistance <= this.LookRadius && enemyScore < this.getScore())
            {
                // Another enemy has lower score and within 5 radius.
                this.Target = closestEnemy.transform;
                return;
            }
        }
        
        // Thirdly, food
        closestFood = GetClosestObject(GameManager.get().getFood());
        if (closestFood != null)
        {
            this.Target = closestFood.transform;
        }
    }
    
    // Rotate enemy towards at target
    void FaceTarget()
    {
        Vector3 direction = (Target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 1f);
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
