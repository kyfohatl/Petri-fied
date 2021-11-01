using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawn : SpawnerController
{
	// Previous cycle entity totals
	public float SpeedBias = 1f / 3f;
	public float MagnetBias = 1f / 3f;
	public float InvincibilityBias = 1f / 3f;
	// Normalised probability vector
	Vector3 probabilityVector;
	
	// Called on start-up of game
	void Awake()
	{
		this.ProcSpawner = GameObject.FindWithTag("Spawner");
		this.probabilityVector = new Vector3(SpeedBias, MagnetBias, InvincibilityBias);
		this.probabilityVector = this.probabilityVector.normalized;
	}
	
	// Called before first frame update
	void Start()
	{
		
	}
	
	// Generate a power-up upon death of an enemy.
	public void DeathGeneratedPowerUp()
	{
		int currentPowerUpCount = this.ProcSpawner.GetComponent<ProceduralSpawner>().powerUpCount;
		int possibleSpawn = NewSpawnCount(currentPowerUpCount);
		if (possibleSpawn >= 1)
		{
			Vector3 spawnOrigin = getRandomPosition();
			Transform parent = this.transform.Find("PowerUps");
			GameObject newPowerUp = Instantiate(this.prefabToSpawn, spawnOrigin, Random.rotation, parent) as GameObject;
			DetermineProbabilityVector();
			int powerUpType = ProbabilityVectorToType(this.probabilityVector);
			newPowerUp.GetComponent<PowerUpManager>().setType(powerUpType);
			GameManager.AddPowerUp(newPowerUp.GetInstanceID(), newPowerUp);
			this.ProcSpawner.GetComponent<ProceduralSpawner>().powerUpCount += 1;
		}
	}
	
	// Function to determine probability of power up type, based on prevalance of certain ratios
	void DetermineProbabilityVector()
	{
		// Determine biases for each power up type
		int totalEnemies = this.ProcSpawner.GetComponent<ProceduralSpawner>().enemyCount;
		int enemiesMax = GetComponent<EnemySpawn>().spawnMax;
		float enemyRatio = totalEnemies / enemiesMax;
		
		// Now deal with lots of food
		int totalFood = this.ProcSpawner.GetComponent<ProceduralSpawner>().foodCount;
		int foodMax = GetComponent<FoodSpawn>().spawnMax;
		float foodRatio = totalFood / foodMax;
		
		int totalSuperFood = this.ProcSpawner.GetComponent<ProceduralSpawner>().superFoodCount;
		int superFoodMax = GetComponent<SuperFoodSpawn>().spawnMax;
		float superFoodRatio = totalSuperFood / superFoodMax;
		
		// Map the 8 ratio possibilities with respect to 50/50's
		if (foodRatio > 0.5f && superFoodRatio > 0.5f && enemyRatio > 0.5f)
		{
			this.probabilityVector.x += superFoodRatio; // increase speed
			this.probabilityVector.y += foodRatio; // increase magnet
			this.probabilityVector.z += enemyRatio; // increase invin
		}
		else if (foodRatio > 0.5f && superFoodRatio > 0.5f && enemyRatio < 0.5f)
		{
			this.probabilityVector.x += foodRatio + superFoodRatio; // increase speed
			this.probabilityVector.y += foodRatio; // increase magnet
		}
		else if (foodRatio > 0.5f && superFoodRatio < 0.5f && enemyRatio > 0.5f)
		{
			this.probabilityVector.z += enemyRatio; // increase invin
		}
		else if (foodRatio > 0.5f && superFoodRatio < 0.5f && enemyRatio < 0.5f)
		{
			this.probabilityVector.y += superFoodRatio + foodRatio + enemyRatio; // increase magnet
		}
		else if (foodRatio < 0.5f && superFoodRatio > 0.5f && enemyRatio > 0.5f)
		{
			this.probabilityVector.x += superFoodRatio; // increase speed
			this.probabilityVector.z += enemyRatio; // increase invin
		}
		else if (foodRatio < 0.5f && superFoodRatio > 0.5f && enemyRatio < 0.5f)
		{
			this.probabilityVector.x += superFoodRatio + foodRatio + enemyRatio; // increase speed
		}
		else if (foodRatio < 0.5f && superFoodRatio < 0.5f && enemyRatio > 0.5f)
		{
			this.probabilityVector.z += superFoodRatio + foodRatio + enemyRatio; // increase invin
		}
		else if (foodRatio < 0.5f && superFoodRatio < 0.5f && enemyRatio < 0.5f)
		{
			this.probabilityVector.x += superFoodRatio; // increase speed
			this.probabilityVector.y += foodRatio; // increase magnet
			this.probabilityVector.z += enemyRatio; // increase invin
		}
		else
		{
			// Would be very rare
			this.probabilityVector = new Vector3(1f, 1f, 1f);
		}
		
		// Now apply biases and normalise the vector to get final probabilities
		this.probabilityVector.x = this.probabilityVector.x * SpeedBias;
		this.probabilityVector.y = this.probabilityVector.y * MagnetBias;
		this.probabilityVector.z = this.probabilityVector.z * InvincibilityBias;
		this.probabilityVector = this.probabilityVector.normalized;
	}
	
	// Function to return an int from a given probability vector
	public int ProbabilityVectorToType(Vector3 vec)
	{
		// Try to ensure 25% of all power ups remainly evenly random
		if (Random.Range(0,4) == 3)
		{
			return 3;
		}
		
		if (Mathf.Abs(vec.x + vec.y + vec.z - 1f) > 0.01f) // allows some tolerance for slightly inexact prob vector
		{
			vec = vec.normalized;
		}
		// Now run a simple comp test to get type
		float testValue = Random.value;
		if (testValue >= 0 && testValue < vec.x)
		{
			return 0; // speed
		}
		else if (testValue >= vec.x && testValue < vec.y)
		{
			return 1; // magnet
		}
		else if (testValue >= vec.y)
		{
			return 2; // inivincibility
		}
		return 3; // random
	}
	
	// Function to generate power ups throughout the scene
	public IEnumerator GeneratePowerUp()
	{
		// Initial wait before first spawn
		yield return new WaitForSeconds(this.timeBetweenSpawns);
		while (this.enabled)
		{
			int currentPowerUpCount = this.ProcSpawner.GetComponent<ProceduralSpawner>().powerUpCount;
			int possibleSpawn = NewSpawnCount(currentPowerUpCount);
			for (int i = 0; i < possibleSpawn; i++)
			{
				// Generate the power up
				Vector3 spawnOrigin = getRandomPosition();
				Transform parent = this.transform.Find("PowerUps");
				GameObject newPowerUp = Instantiate(this.prefabToSpawn, spawnOrigin, Random.rotation, parent) as GameObject;
				DetermineProbabilityVector();
				int powerUpType = ProbabilityVectorToType(this.probabilityVector);
				newPowerUp.GetComponent<PowerUpManager>().setType(powerUpType);
				GameManager.AddPowerUp(newPowerUp.GetInstanceID(), newPowerUp);
				this.ProcSpawner.GetComponent<ProceduralSpawner>().powerUpCount += 1;
			}
			
			// Now wait for the time between spawns to complete
			yield return new WaitForSeconds(this.timeBetweenSpawns);
		}
	}
}
