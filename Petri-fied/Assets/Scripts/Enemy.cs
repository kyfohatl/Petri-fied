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
		if (Target == null)
		{
			DetermineTarget();
		}

		// Move towards active target (needs definitte work: currently doesn't re-target until it's target has been consumed, you can imagine why that's bad)
		if (Target != null)
		{
			FaceTarget();
			transform.position += 2f * getSpeedMultiplier() * getPowerUpSpeedMultiplier() * transform.forward * Time.deltaTime / transform.localScale.x;
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
		float expectedEnemyScore = 0f;

		GameObject closestFood = null;
		float expectedFoodScore = 0f;

		GameObject newTarget = null;
		float bestExpected = expectedTargetScore();
		
		int playerScore = 0;
		if (Player != null)
		{
			playerScore = Player.GetComponent<IntelligentAgent>().getScore();
		}
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
		var possibleEnemies = GameManager.get().getEnemies();
		if (possibleEnemies != null)
		{
			foreach (KeyValuePair<int, GameObject> enemyClone in possibleEnemies)
			{
				int enemyScore = enemyClone.Value.GetComponent<IntelligentAgent>().getScore();
				float enemyDistance = Vector3.Distance(transform.position, enemyClone.Value.transform.position);
				expectedEnemyScore = (float)enemyScore / enemyDistance;
				
				if (enemyDistance <= this.getLockOnRadius()
					&& enemyScore < this.getScore()
					&& expectedEnemyScore > bestExpected)
				{
					// Another enemy has lower score and within lock-on radius.
					bestExpected = expectedEnemyScore;
					newTarget = closestEnemy;
				}
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
			return Target.GetComponent<IntelligentAgent>().getScore() / dist;
		}
		else if (this.Target.tag == "Player")
		{
			return Target.GetComponent<IntelligentAgent>().getScore() / dist;
		}
		else if (this.Target.tag == "PowerUp")
		{
			// Return a magic value representing the value of a power-up (equal to agent's current score)
			return getScore(); // i.e. expected to double it's score with one power up
		}

		// Target must be food otherwise
		return this.getFoodGrowthMultiplier() / dist;
	}
}
