using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperFoodSpawn : SpawnerController
{
	// Rate reduction multiplier
	public float RateReductionMultiplier = 1f;
	public float MinSpawnDelta = 5f;
	public float LimitIncreaseMultiplier = 2f;
	private int initialSpawnLimit = 20;
	
	// Start is called before the first frame update
	void Awake()
	{
		this.ProcSpawner = GameObject.FindWithTag("Spawner");
		this.initialSpawnLimit = this.spawnLimit;
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}
	
	// Function to generate food, will try to spawn some food around the player first then randomly in arena
	public IEnumerator GenerateSuperFood()
	{
		while(this.enabled)
		{
			// Decrease the spawn-rate by a subtraction of a multiple of the log10Score
			float log10Score = this.ProcSpawner.GetComponent<ProceduralSpawner>().getPlayerLog10ScaleFactor();
			float rateReduction = this.RateReductionMultiplier * (log10Score - 1f); // starts at 0
			float deltaSpawn = this.timeBetweenSpawns - rateReduction;
			if (deltaSpawn < this.MinSpawnDelta)
			{
				deltaSpawn = this.MinSpawnDelta;
			}
			
			// Increase maximum amount of allowable superfood in scene
			float limitIncrease = this.LimitIncreaseMultiplier * (log10Score - 1f);
			int newLimit = (int)Mathf.Floor(this.initialSpawnLimit + limitIncrease);
			this.spawnLimit = newLimit;
			
			// Generate the super food
			int spawnCount = NewSpawnCount(this.ProcSpawner.GetComponent<ProceduralSpawner>().superFoodCount);
			
			// Loop to spawn one or more objects per soawn - cycle
			for (int i = 0; i < spawnCount; i++)
			{
				// Track the spawned super food
				Vector3 spawnPosition = getRandomPosition();
				Transform parent = this.transform.Find("Supers");
				GameObject newSuperFood = Instantiate(this.prefabToSpawn, spawnPosition, Random.rotation, parent);
				GameManager.AddFood(newSuperFood.GetInstanceID(), newSuperFood);
				GameManager.AddSuperFood(newSuperFood.GetInstanceID(), newSuperFood);
				this.ProcSpawner.GetComponent<ProceduralSpawner>().superFoodCount += 1;
			}
			
			// Now wait for the time between spawns to complete
			yield return new WaitForSeconds(deltaSpawn);
		}
	}
}
