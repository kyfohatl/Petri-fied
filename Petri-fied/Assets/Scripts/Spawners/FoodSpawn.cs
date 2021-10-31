using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawn : SpawnerController
{
	// Pre-instantiated object pool and initial maximum
	private List<GameObject> foodPool;
	public int initialMaximum = 400; // at mosy 400 food can be in the scene
	public int absoluteMinimum = 50; // at least 50 food will be in the scene once spawned
	public float percentageReducedPerLog = 15f;
	
	// The governing procedural spawner that calls this spawner
	private ProceduralSpawner ProcSpawner;
	
	// Called on start-up of game
	void Awake()
	{
		PreInstantiate();
		this.ProcSpawner = GetComponent<ProceduralSpawner>();
	}
	
	// Start is called before the first frame update, used to pre-instanstiate all food
	void Start()
	{
		
	}
	
	// During load of game
	void PreInstantiate()
	{
		this.foodPool = new List<GameObject>();
		GameObject newFood;
		for (int i = 0; i < initialMaximum; i++)
		{
			Vector3 Target = getRandomPosition(); // this will be lastly decided by the procedural spawner anyway
			newFood = Instantiate(this.prefabToSpawn, this.transform.Find("FoodPellets"));
			newFood.SetActive(false);
			newFood.transform.rotation = Random.rotation;
			this.foodPool.Add(newFood);
		}
	}
	
	// Getter function for foodPool
	public List<GameObject> getFoodPool()
	{
		return this.foodPool;
	}
	
	// Function to generate food, will try to spawn some food around the player first then randomly in arena
	public IEnumerator GenerateFood()
	{
		while(this.enabled)
		{
			// Limit the amount of food spawn to be the initial maximum subtract a multiplier of the log10Score
			float multiplier = (float)this.initialMaximum * this.percentageReducedPerLog / 100f;
			float log10Score = this.ProcSpawner.getPlayerLog10ScaleFactor();
			int limitDeduction = (int)Mathf.Floor(multiplier * log10Score);
			int spawnMaximum = this.initialMaximum - limitDeduction;
			int spawnMinimum = this.absoluteMinimum;
			int newLimit = (int)Mathf.Max(spawnMinimum, spawnMaximum);
			this.spawnLimit = newLimit;
			
			// Now activate and relocate food pellets equal to how many can spawn; prioritise around player first
			int howManyToSpawnAroundPlayer = NewSpawnCount(this.ProcSpawner.foodCount);
			int howManySpawnRandom = NewSpawnCount(this.ProcSpawner.foodCount);
			// Check to see if proposed spawn counts can still be spawned
			if (howManyToSpawnAroundPlayer + this.ProcSpawner.foodCount > newLimit)
			{
				howManyToSpawnAroundPlayer = newLimit - this.ProcSpawner.foodCount;
				howManySpawnRandom = 0;
			}
			else if (howManySpawnRandom + howManyToSpawnAroundPlayer + this.ProcSpawner.foodCount > newLimit)
			{
				howManySpawnRandom = newLimit - this.ProcSpawner.foodCount - howManyToSpawnAroundPlayer;
			}
			
			// Now activate and relocate food pellets equal to how many can spawn; prioritise around player first
			for (int i = 0; i < howManyToSpawnAroundPlayer; i++)
			{
				// Determine the final spawn position
				float lockOnRadius = this.ProcSpawner.Player.GetComponent<IntelligentAgent>().getLockOnRadius();
				Debug.Log("enabled boi");
				Vector3 playerPos = this.ProcSpawner.Player.transform.position;
				Vector3 spawnPosition = getRandomPosition(lockOnRadius, playerPos);
				GameObject newFood = this.transform.Find("FoodPellets").GetChild(this.ProcSpawner.foodCount).gameObject;
				if (newFood.activeInHierarchy)
				{
					Debug.Log("trying to use already active food");
					Debug.Break();
				}
				// Check to ensure the spawn position in within the spawn arena (encourages staying away from edge)
				if (this.ProcSpawner.withinArena(spawnPosition))
				{
					newFood.transform.position = spawnPosition;
				}
				else
				{
					newFood.transform.position = getRandomPosition();
				}
				Debug.Log("Planned: " + newFood.transform.position);
				
				// Adjust scale to make food larger as the player grows
				float newScaleMultiplier = Mathf.Log(log10Score); // log of log to limit growth
				newScaleMultiplier = Random.Range(1f, 1f + newScaleMultiplier);
				Vector3 newScale = newScaleMultiplier * new Vector3(0.5f, 0.5f, 0.5f);
				newFood.transform.localScale = newScale;
				
				// Finally make food active and add to game manager dictionary
				newFood.SetActive(true);
				GameManager.AddFood(newFood.GetInstanceID(), newFood);
				this.ProcSpawner.foodCount += 1;
			}
			
			// Now activate food pellets randomly around the arena
			for (int i = 0; i < howManySpawnRandom; i++)
			{
				// Determine the final spawn position
				Vector3 spawnPosition = getRandomPosition();
				GameObject newFood = this.transform.Find("FoodPellets").GetChild(this.ProcSpawner.foodCount).gameObject;
				if (newFood.activeInHierarchy)
				{
					Debug.Log("trying to use already active food");
					Debug.Break();
				}
				newFood.transform.position = spawnPosition;
				Debug.Log("Rando: " + newFood.transform.position);
				
				// Adjust scale to make food larger as the player grows
				float newScaleMultiplier = Mathf.Log(log10Score); // log of log to limit growth
				newScaleMultiplier = Random.Range(1f, 1f + newScaleMultiplier);
				Vector3 newScale = newScaleMultiplier * new Vector3(0.5f, 0.5f, 0.5f);
				
				// Finally make food active and add to game manager dictionary
				newFood.SetActive(true);
				GameManager.AddFood(newFood.GetInstanceID(), newFood);
				this.ProcSpawner.foodCount += 1; //
			}
			
			// Now wait for the time between spawns to complete
			yield return new WaitForSeconds(this.timeBetweenSpawns);
		}
	}
}
