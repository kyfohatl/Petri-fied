using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Enemy : IntelligentAgent
{
	// Track player
	private GameObject Player = null;

	// Determination timer
	private float determineTimer = 0f;
	[SerializeField] private float AggressionMultiplier = 1f;

	// Start is called before the first frame update
	void Start()
	{
		StartLife();
		this.setName(GenerateRandomName());
		this.name = "Enemy: " + this.getName();
		this.Player = GameObject.FindGameObjectWithTag("Player");

		// Determine aggresion of this enemy and apply initial difficulty sliders
		this.AggressionMultiplier = Mathf.Abs(normalRandom(0f, 1.4826f)); // this stddev produces 50% of agents above/below aggresion mult of 1f
		ApplyDifficultySliders();
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
		int prefixAlphaCount = UnityEngine.Random.Range(1,5); // [1-4]
		int digitCount = UnityEngine.Random.Range(1,7); // [1-6]
		int suffixAlphaCount = UnityEngine.Random.Range(1,4); // [1-3]
		// e.g. A1-X or ABC123456-XYZ etc

		// Prefix characters
		for (int i = 0; i < prefixAlphaCount; i++)
		{
			randomString += (char)UnityEngine.Random.Range('A', 'Z');
		}

		// Digit characters
		for (int i = 0; i < digitCount; i++)
		{
			randomString += (char)UnityEngine.Random.Range('0', '9');
		}
		randomString += "-"; // add dash between digits and suffix

		// Suffix characters
		for (int i = 0; i < suffixAlphaCount; i++)
		{
			randomString += (char)UnityEngine.Random.Range('A', 'Z');
		}

		return randomString;
	}

	// Function to determine target
	public void DetermineTarget()
	{
		// Initial state: no change to target
		GameObject newTarget = null;
		float bestExpected = ExpectedTargetScore(this.Target);

		//  Calculate expected score of targetting player
		int playerScore = 0;
		float playerDistance = 0f;
		float expectedPlayerScore = 0f;

		if (this.Player != null)
		{
			playerScore = Player.GetComponent<IntelligentAgent>().getScore();
			playerDistance = Vector3.Distance(transform.position, this.Player.transform.position);
			expectedPlayerScore = ExpectedTargetScore(this.Player);

			if (playerDistance <= this.getLockOnRadius()
				&& playerScore < this.getScore()
				&& expectedPlayerScore > bestExpected)
			{
				bestExpected = expectedPlayerScore;
				newTarget = this.Player;
			}
		}
		// Other enemies
		Dictionary<int, GameObject> possibleEnemies = GameManager.get().getEnemies();
		float expectedEnemyScore = 0f;

		if (possibleEnemies != null)
		{
			foreach (KeyValuePair<int, GameObject> enemyClone in possibleEnemies)
			{
				int enemyScore = enemyClone.Value.GetComponent<IntelligentAgent>().getScore();
				float enemyDistance = Vector3.Distance(transform.position, enemyClone.Value.transform.position);
				expectedEnemyScore = ExpectedTargetScore(enemyClone.Value);

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
		// Closest food entity
		GameObject closestFood = null;
		closestFood = GetClosestObject(GameManager.get().getFood());
		float expectedFoodScore = 0f;

		if (closestFood != null)
		{
			float foodDistance = Vector3.Distance(transform.position, closestFood.transform.position);
			expectedFoodScore = ExpectedTargetScore(closestFood);

			if (expectedFoodScore > bestExpected)
			{
				bestExpected = expectedFoodScore;
				newTarget = closestFood;
			}
		}
		// Closest super food entity
		GameObject closestSuperFood = null;
		closestSuperFood = GetClosestObject(GameManager.get().getSuperFood());
		float expectedSuperFoodScore = 0f;

		if (closestSuperFood != null)
		{
			float superFoodDistance = Vector3.Distance(transform.position, closestSuperFood.transform.position);
			expectedSuperFoodScore = ExpectedTargetScore(closestSuperFood);

			if (expectedSuperFoodScore > bestExpected)
			{
				bestExpected = expectedSuperFoodScore;
				newTarget = closestSuperFood;
			}
		}
		// Closest power up entities
		GameObject closestPowerUp = null;
		closestPowerUp = GetClosestObject(GameManager.get().getPowerUps());
		float expectedPowerUpScore = 0f;

		if (closestPowerUp != null)
		{
			float powerUpDistance = Vector3.Distance(transform.position, closestPowerUp.transform.position);
			expectedPowerUpScore = ExpectedTargetScore(closestPowerUp);

			if (powerUpDistance <= this.getLockOnRadius() && expectedPowerUpScore > bestExpected)
			{
				bestExpected = expectedPowerUpScore;
				newTarget = closestPowerUp;
			}
		}

		// Finally, update Target
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
	public float ExpectedTargetScore(GameObject target)
	{
		// Exit early if target has been eaten already
		if (target == null)
		{
			return 0f;
		}

		// Calculate distance to the current target
		float dist = Vector3.Distance(transform.position, target.transform.position);
		float mySpeed = getSpeedMultiplier() * getPowerUpSpeedMultiplier() / transform.localScale.x;
		float expectedTravelTime = dist / mySpeed;

		if (target.tag == "Enemy" || target.tag == "Player")
		{
			int targetScore = target.GetComponent<IntelligentAgent>().getScore();
			if (targetScore >= this.Score || target.GetComponent<IntelligentAgent>().isInvincible())
			{
				return 0f; // current target cannot be eaten
			}

			float targetSpeed = target.GetComponent<IntelligentAgent>().getSpeedMultiplier() * target.GetComponent<IntelligentAgent>().getPowerUpSpeedMultiplier() / target.transform.localScale.x;

			if (targetSpeed > mySpeed)
			{
				targetScore = Mathf.FloorToInt((targetScore / 2) * (mySpeed / targetSpeed));
				expectedTravelTime = (dist * 2f) / (mySpeed / targetSpeed);
			}
			else
			{
				expectedTravelTime = dist / (mySpeed - targetSpeed);
			}

			return this.AggressionMultiplier * targetScore / expectedTravelTime;
		}
		else if (target.tag == "PowerUp")
		{
			if (!target.GetComponent<MeshRenderer>().enabled)
			{
				return 0f; // power up is no longer visible
			}
			// Return a magic value representing the 'value' of a power-up, equal to player's score and rewards smaller travel times
			return this.AggressionMultiplier * this.Score / (expectedTravelTime * expectedTravelTime);
		}
		else if (target.tag == "SuperFood")
		{
			return Mathf.Max((float)this.Score / 10f, 10f) / expectedTravelTime; // refer to IntAgent OnTriggerEnter with superfood tag
		}
		else if (target.tag == "Food")
		{
			return this.transform.localScale.x * this.getFoodGrowthMultiplier() / expectedTravelTime; // is meant to get easier the bigger you are
		}
		else
		{
			// If here, there is an issue
			Debug.Log(this.name + "Has an issue with targeting, unknown tag encountered.");
			Debug.Break();
			return 0f;
		}
	}

	// Function to apply modified game difficulty sliders
	public void ApplyDifficultySliders()
	{
		setSpeedMultiplier(getSpeedMultiplier() * GameManager.enemySpeedBoost);
		setFoodGrowthMultiplier(getFoodGrowthMultiplier() * GameManager.enemyGrowthBoost);
		this.AggressionMultiplier *= GameManager.enemyAggressionMultiplier;
	}
}
