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
		this.name = "Enemy: " + this.Name;
		this.Player = GameObject.FindGameObjectWithTag("Player");
		Leaderboard.Instance.AddAgent((IntelligentAgent)this);
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
		if (this.Target == null)
		{
			DetermineTarget();
		}
		
		// Move towards active target (needs definitte work: currently doesn't re-target until it's target has been consumed, you can imagine why that's bad)
		if (this.Target != null)
		{
			// Enemy is moving
			gameObject.GetComponent<IsMoving>().isMoving = true;
			
			FaceTarget();
			transform.position += 3f * getSpeedMultiplier() * getPowerUpSpeedMultiplier() * transform.forward * Time.deltaTime / transform.localScale.x;
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
		// Initial state: no change to target
		GameObject newTarget = null;
		float bestExpected = expectedTargetScore(this.Target);

		//  First, calculate hunting player value
		int playerScore = 0;
		float playerDistance = 0f;
		float expectedPlayerScore = 0f;
		
		if (this.Player != null)
		{
			playerScore = Player.GetComponent<IntelligentAgent>().getScore();
			playerDistance = Vector3.Distance(transform.position, this.Player.transform.position);
			expectedPlayerScore = expectedTargetScore(this.Player);
			
			if (playerDistance <= this.getLockOnRadius()
				&& playerScore < this.getScore()
				&& expectedPlayerScore > bestExpected)
			{
				bestExpected = expectedPlayerScore;
				newTarget = this.Player;
			}
		}
		// Secondly, other enemies
		var possibleEnemies = GameManager.get().getEnemies();
		float expectedEnemyScore = 0f;
		
		if (possibleEnemies != null)
		{
			foreach (KeyValuePair<int, GameObject> enemyClone in possibleEnemies)
			{
				int enemyScore = enemyClone.Value.GetComponent<IntelligentAgent>().getScore();
				float enemyDistance = Vector3.Distance(transform.position, enemyClone.Value.transform.position);
				expectedEnemyScore = expectedTargetScore(enemyClone.Value);
				
				if (enemyDistance <= this.getLockOnRadius()
					&& enemyScore < this.Score
					&& expectedEnemyScore > bestExpected)
				{
					// Another enemy has lower score and within lock-on radius.
					bestExpected = expectedEnemyScore;
					newTarget = enemyClone.Value;
				}
			}
		}
		// Thirdly, food entities
		GameObject closestFood = null;
		closestFood = GetClosestObject(GameManager.get().getFood());
		float expectedFoodScore = 0f;
		
		if (closestFood != null)
		{
			float foodDistance = Vector3.Distance(transform.position, closestFood.transform.position);
			expectedFoodScore = expectedTargetScore(closestFood);

			if (expectedFoodScore > bestExpected)
			{
				bestExpected = expectedFoodScore;
				newTarget = closestFood;
			}
		}
		// Fourthly, power ups
		GameObject closestPowerUp = null;
		closestPowerUp = GetClosestObject(GameManager.get().getPowerUps());
		float expectedPowerUpScore = 0f;
		
		if (closestPowerUp != null)
		{
			float powerUpDistance = Vector3.Distance(transform.position, closestPowerUp.transform.position);
			expectedPowerUpScore = expectedTargetScore(closestPowerUp);
			
			if (expectedPowerUpScore > bestExpected)
			{
				bestExpected = expectedPowerUpScore;
				newTarget = closestPowerUp;
			}
		}
		// Lastly, update Target
		if (newTarget != null)
		{
			this.Target = newTarget;
		}
		
		//// debugging lines
		if (this.Target == null && closestFood != null)
		{
			Debug.Log(this.name + " has an issue with targetting");
			Debug.Break();
		}
	}

	// Function to calculate expected score by going for target
	public float expectedTargetScore(GameObject target)
	{
		// Exit early if target has been eaten already
		if (target == null)
		{
			return 0f;
		}

		// Calculate distance to the current target
		float dist = Vector3.Distance(transform.position, target.transform.position);

		if (target.tag == "Enemy" || target.tag == "Player")
		{
			int targetScore = target.GetComponent<IntelligentAgent>().getScore();
			if (targetScore >= this.Score)
			{
				// current target cannot be eaten
				return 0f;
			}
			return targetScore / dist;
		}
		else if (target.tag == "PowerUp")
		{
			// Return a magic value representing the 'value' of a power-up (score^2 / dist^2 rewards close power ups)
			return this.Score * this.Score / (dist * dist);
		}
		else if (target.tag == "SuperFood")
		{
			return Mathf.Max(this.Score / 6, 10) / dist; // refer to intelligent agent OnTriggerEnter with superfood tag
		}
		else if (target.tag == "Food")
		{
			return this.getFoodGrowthMultiplier() / dist;
		}
		else
		{
			// If here, there is an issue
			Debug.Log(this.name + "Has an issue with targeting, unknown tag encountered.");
			Debug.Break();
			return 0f;
		}
	}
}
