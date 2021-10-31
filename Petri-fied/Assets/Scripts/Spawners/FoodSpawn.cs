using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawn : SpawnerController
{
	// Pre-instantiated object pool and initial maximum
	public static FoodSpawn instance;
	private List<GameObject> foodPool;
	public int spawnAroundPlayerMax = 3;
	public int initialMaximum = 300;
	public int absoluteMinimum = 50;
	public float percentageReducedPerLog = 15f;
	
	// Called on start-up of game
	void Awake()
	{
		instance = this;
		PreInstantiate();
		this.ProcSpawner = GameObject.FindWithTag("Spawner");
		if (this.initialMaximum != this.spawnLimit)
		{
			int larger = (int)Mathf.Max(this.initialMaximum, this.spawnLimit);
			this.initialMaximum = larger;
			this.spawnLimit = larger;
		}
	}
	
	// Start is called before the first frame update
	void Start()
	{
		
	}
	
	// Get the first inActive Food Pellet
	public GameObject GetInactiveFood()
	{
		for (int i = 0; i < initialMaximum; i++)
		{
			if (!foodPool[i].activeInHierarchy)
			{
				return foodPool[i];
			}
		}
		return null;
	}
	
	// During load of game
	void PreInstantiate()
	{
		this.foodPool = new List<GameObject>();
		GameObject newFood;
		Transform parent = this.gameObject.transform.Find("FoodPellets");
		for (int i = 0; i < this.initialMaximum; i++)
		{
			Vector3 Target = getRandomPosition(); // this will be lastly decided by the procedural spawner anyway
			newFood = Instantiate(this.prefabToSpawn, parent);
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
			float log10Score = this.ProcSpawner.GetComponent<ProceduralSpawner>().getPlayerLog10ScaleFactor();
			int limitDeduction = (int)Mathf.Floor(multiplier * log10Score);
			int spawnMaximum = this.initialMaximum - limitDeduction;
			int spawnMinimum = this.absoluteMinimum;
			int newLimit = (int)Mathf.Max(spawnMinimum, spawnMaximum);
			this.spawnLimit = newLimit;
			
			// Determine how many of each food type will spawn
			int currentActiveFood = this.ProcSpawner.GetComponent<ProceduralSpawner>().foodCount;
			int howManyToSpawnAroundPlayer = NewSpawnCount(currentActiveFood);
			if (howManyToSpawnAroundPlayer > this.spawnAroundPlayerMax)
			{
				howManyToSpawnAroundPlayer = this.spawnAroundPlayerMax;
			}
			int howManySpawnRandom = NewSpawnCount(currentActiveFood);
			// Check to see if proposed spawn counts can still be spawned
			if (howManyToSpawnAroundPlayer + currentActiveFood > newLimit)
			{
				howManyToSpawnAroundPlayer = newLimit - currentActiveFood;
				howManySpawnRandom = 0;
			}
			else if (howManySpawnRandom + howManyToSpawnAroundPlayer + currentActiveFood > newLimit)
			{
				howManySpawnRandom = newLimit - currentActiveFood - howManyToSpawnAroundPlayer;
			}
			
			// Now activate and relocate food pellets equal to how many can spawn; prioritise around player first
			for (int i = 0; i < howManyToSpawnAroundPlayer; i++)
			{
				// Determine the final spawn position
				float lockOnRadius = this.ProcSpawner.GetComponent<ProceduralSpawner>().Player.GetComponent<IntelligentAgent>().getLockOnRadius();
				Vector3 playerPos = this.ProcSpawner.GetComponent<ProceduralSpawner>().Player.transform.position;
				Vector3 spawnPosition = getRandomPosition(lockOnRadius, playerPos);
				int index = this.ProcSpawner.GetComponent<ProceduralSpawner>().foodCount;
				GameObject newFood = instance.GetInactiveFood();
				if (newFood == null)
				{
					break;
				}
				if (newFood.activeInHierarchy)
				{
					Debug.Log("trying to use already active food");
				}
				// Check to ensure the spawn position in within the spawn arena (encourages staying away from edge)
				if (this.ProcSpawner.GetComponent<ProceduralSpawner>().withinArena(spawnPosition))
				{
					newFood.transform.position = spawnPosition;
				}
				else
				{
					newFood.transform.position = getRandomPosition();
				}
				
				// Adjust scale to make food larger as the player grows
				float newScaleMultiplier = Mathf.Log(log10Score); // log of log to limit growth
				newScaleMultiplier = Random.Range(1f, 1f + newScaleMultiplier);
				Vector3 newScale = newScaleMultiplier * new Vector3(0.5f, 0.5f, 0.5f);
				newFood.transform.localScale = newScale;
				
				// Finally make food active and add to game manager dictionary
				newFood.SetActive(true);
				GameManager.AddFood(newFood.GetInstanceID(), newFood);
				this.ProcSpawner.GetComponent<ProceduralSpawner>().foodCount += 1;
			}
			
			// Now activate food pellets randomly around the arena
			for (int i = 0; i < howManySpawnRandom; i++)
			{
				// Determine the final spawn position
				Vector3 spawnPosition = getRandomPosition();
				int index = this.ProcSpawner.GetComponent<ProceduralSpawner>().foodCount;
				GameObject newFood = instance.GetInactiveFood();
				if (newFood == null)
				{
					break;
				}
				if (newFood.activeInHierarchy)
				{
					Debug.Log("trying to use already active food");
				}
				newFood.transform.position = spawnPosition;
				
				// Adjust scale to make food larger as the player grows
				float newScaleMultiplier = Mathf.Log(log10Score); // log of log to limit growth
				newScaleMultiplier = Random.Range(1f, 1f + newScaleMultiplier);
				Vector3 newScale = newScaleMultiplier * new Vector3(0.5f, 0.5f, 0.5f);
				
				// Finally make food active and add to game manager dictionary
				newFood.SetActive(true);
				GameManager.AddFood(newFood.GetInstanceID(), newFood);
				this.ProcSpawner.GetComponent<ProceduralSpawner>().foodCount += 1;
			}
			
			// Now wait for the time between spawns to complete
			yield return new WaitForSeconds(this.timeBetweenSpawns);
		}
	}
}
