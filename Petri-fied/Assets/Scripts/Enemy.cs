using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Enemy : IntelligentAgent
{
	// Track player
	public GameObject Player;
	
	// Determination timer
	private float determineTimer = 0f;
	
	// Start is called before the first frame update
	void Start()
	{
		StartLife();
		this.Name = GenerateRandomName();
		this.Player = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	void Update()
	{
		DecayScore();
		UpdateSize();
		
		// Check once every second in case of target updates / closer better loot etc
		determineTimer += Time.deltaTime;
		if (determineTimer >= 1f)
		{
			determineTimer = 0f;
			DetermineTarget();
		}
		
		// If no current target, determine next
		if (Target == null)
		{
			DetermineTarget();
		}
		
		// Move towards active target (needs definitte work: currently doesn't re-target until it's target has been consumed, you can imagine why that's bad)
		if (Target != null)
		{
			FaceTarget();
			transform.position += 3f * base.getSpeedMultiplier() * transform.forward * Time.deltaTime / transform.localScale.x;
		}
	}
	
	// Function to generate a random name
	private string GenerateRandomName()
	{
		string randomString = "";
		int digitCount = 3;
		
		randomString += (char)UnityEngine.Random.Range('A', 'Z');
		for (int i = 0; i < digitCount; i++)
		{
			randomString += (char)UnityEngine.Random.Range('0', '9');
		}
		randomString += "-" + (char)UnityEngine.Random.Range('A', 'Z');
		
		return randomString;
	}
	
	// Function to determine target
	public void DetermineTarget()
	{
		GameObject closestEnemy = null;
		float expectedEnemyScore;
		
		GameObject closestFood = null;
		float expectedFoodScore;
		
		GameObject newTarget = null;
		float bestExpected = expectedTargetScore();
		
		int playerScore = Player.GetComponent<Player>().getScore();
		float playerDistance = Vector3.Distance(transform.position, Player.transform.position);
		float expectedPlayerScore = (float)playerScore / playerDistance;
		
		// Prioritise player first
		if (playerDistance <= this.getLockOnRadius()
			&& playerScore < this.getScore()
			&& expectedPlayerScore > bestExpected)
		{
			bestExpected = expectedPlayerScore;
			newTarget = Player;
		}
		
		// Secondly, other enemies
		closestEnemy = GetClosestObject(GameManager.get().getEnemies());
		if (closestEnemy != null)
		{
			int enemyScore = closestEnemy.GetComponent<Enemy>().getScore();
			float enemyDistance = Vector3.Distance(transform.position, closestEnemy.transform.position);
			expectedEnemyScore = (float)enemyScore / enemyDistance;
			
			if (enemyDistance <= this.getLockOnRadius()
				&& enemyScore < this.getScore()
				&& expectedEnemyScore > bestExpected)
			{
				// Another enemy has lower score and within 5 radius.
				bestExpected = expectedEnemyScore;
				newTarget = closestEnemy;
			}
		}
		
		// Thirdly, food entities
		closestFood = GetClosestObject(GameManager.get().getFood());
		if (closestFood != null)
		{
			float foodDistance = Vector3.Distance(transform.position, closestFood.transform.position);
			expectedFoodScore = this.getFoodGrowthMultiplier() / foodDistance;
			
			if (expectedFoodScore > bestExpected)
			{
				bestExpected = expectedFoodScore;
				newTarget = closestFood;
			}
		}
		
		// Lastly, update Target
		if (newTarget != null)
		{
			this.Target = newTarget;
		}
	}
	
	// Function to calculate expected score by going for target
	public float expectedTargetScore()
	{
		if (this.Target == null)
		{
			return 0f;
		}
		
		// Calculate distance to the current target
		float dist = Vector3.Distance(transform.position, this.getTarget().transform.position);
		
		if (this.Target.tag == "Enemy")
		{
			return Target.GetComponent<Enemy>().getScore() / dist;
		}
		else if (this.Target.tag == "Player")
		{
			return Target.GetComponent<Player>().getScore() / dist;
		}
		
		// Assume target is food otherwise
		return this.getFoodGrowthMultiplier() / dist;
	}
}
