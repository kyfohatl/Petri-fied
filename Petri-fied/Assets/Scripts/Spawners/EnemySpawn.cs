using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : SpawnerController
{
	// Average position of enemy
	private Vector3 averageEnemyPosition;
	public float increaseEnemiesMultiplier = 1f;
	public int initialMaximum = 16;
	
	// Trackers used to store data on
	private Vector3 opposingSphereOrigin;
	private float opposingSphereRadius;
	
	// Called on start-up of game
	void Awake()
	{
		this.ProcSpawner = GameObject.FindWithTag("Spawner");
		this.Arena = GameObject.FindWithTag("Arena");
		getArenaDimensions();
		if (this.initialMaximum != this.spawnLimit)
		{
			int larger = (int)Mathf.Max(this.initialMaximum, this.spawnLimit);
			this.initialMaximum = larger;
			this.spawnLimit = larger;
		}
		this.opposingSphereOrigin = this.Arena.transform.position;
		this.opposingSphereRadius = this.Arena.GetComponent<ArenaSize>().ArenaRadius;
	}
	
	// Start is called before the first frame update
	void Start()
	{
		
	}
	
	// Function to convert determine the central position of all enemies
	void DetermineAveragePosition()
	{
		Vector3 averagePos = new Vector3(0f, 0f, 0f);
		Dictionary<int, GameObject> possibleEnemies = GameManager.get().getEnemies();
		int count = 0;
		if (possibleEnemies == null)
		{
			this.averageEnemyPosition = averagePos;
			return;
		}
		// Otherwise, loop through all existing enemies
		foreach (KeyValuePair<int, GameObject> clone in possibleEnemies)
		{
			averagePos += clone.Value.transform.position;
			count += 1;
		}
		averagePos /= count;
		this.averageEnemyPosition = averagePos;
	}
	
	// Function to get the distance of the furthest enemy from the average enemy position
	private float GetFurthestEnemyDistance()
	{
		float maxDist = 0f;
		float epsilon = 1e-4f;
		
		Dictionary<int, GameObject> possibleEnemies = GameManager.get().getEnemies();
		if (possibleEnemies == null)
		{
			return 0f;
		}
		foreach (KeyValuePair<int, GameObject> clone in possibleEnemies)
		{
			Vector3 vectorToClone = clone.Value.transform.position - this.averageEnemyPosition;
			float distSqrToTarget = vectorToClone.sqrMagnitude;
			
			if (distSqrToTarget > maxDist && distSqrToTarget > epsilon)
			{
				maxDist = distSqrToTarget;
			}
		}
		return Mathf.Sqrt(maxDist);
	}
	
	// Funtion to determine the pair largest possible sphere that can fit in the spawn arena
	public void GetOpposingSphere(Vector3 point, float boundingSphereRadius)
	{
		getArenaDimensions();
		Vector3 vectorToPoint = point - this.arenaOrigin;
		Vector3 direction = vectorToPoint.normalized;
		float distToPoint = vectorToPoint.magnitude;
		this.opposingSphereRadius = 0.5f * this.arenaRadius - (boundingSphereRadius - distToPoint);
		this.opposingSphereOrigin = point + (-direction * (boundingSphereRadius + this.opposingSphereRadius));
	}
	
	// Function to retrieve player's current score
	private int getPlayerScore()
	{
		return this.ProcSpawner.GetComponent<ProceduralSpawner>().Player.GetComponent<IntelligentAgent>().getScore();
	}
	
	// Function to generate enemies throughout the scene
	public IEnumerator GenerateEnemy()
	{
		// Initial wait before first spawn
		yield return new WaitForSeconds(this.timeBetweenSpawns);
		while (this.enabled)
		{
			// Determine how much the spawn parameters need to change to facilitate the stage of the game
			float playerScore = getPlayerScore();
			float log10Score = this.ProcSpawner.GetComponent<ProceduralSpawner>().getPlayerLog10ScaleFactor();
			int proposedMax = (int)(this.initialMaximum + this.increaseEnemiesMultiplier * (Mathf.Floor(log10Score) - 1f));
			if (proposedMax > this.spawnLimit)
			{
				this.spawnLimit = proposedMax;
			}
			int currentEnemyCount = this.ProcSpawner.GetComponent<ProceduralSpawner>().enemyCount;
			int randomSpawn = NewSpawnCount(currentEnemyCount);
			randomSpawn = (int)Mathf.Min(1f, randomSpawn);
			// Always try to spawn one enemy completly randomly unless at spawn max already
			for (int i = 0; i < randomSpawn; i++)
			{
				// Generate the enemy wherever
				Vector3 spawnOrigin = getRandomPosition();
				Transform parent = this.transform.Find("Enemies");
				GameObject newEnemy = Instantiate(this.prefabToSpawn, spawnOrigin, Random.rotation, parent);
				GameObject enemyBody = newEnemy.transform.Find("Avatar").gameObject;
				GameManager.AddEnemy(enemyBody.GetInstanceID(), enemyBody);
				this.ProcSpawner.GetComponent<ProceduralSpawner>().enemyCount += 1;
			}
			
			// Now try to spawn enemies in the opposing sphere
			currentEnemyCount = this.ProcSpawner.GetComponent<ProceduralSpawner>().enemyCount;
			int controlledSpawn = NewSpawnCount(currentEnemyCount);
			float furthest = 0f;
			if (controlledSpawn >= 1 && currentEnemyCount > 1)
			{
				DetermineAveragePosition();
				furthest = GetFurthestEnemyDistance();
				GetOpposingSphere(this.averageEnemyPosition, furthest);
			}
			// Loop through the controlled sphere enemies and generate
			for (int i = 0; i < controlledSpawn; i++)
			{
				// The spawn origin can be anywhere in the case of 1 or fewer enemies spawned thus far
				Vector3 spawnOrigin;
				if (furthest == 0f)
				{
					spawnOrigin = getRandomPosition();
				}
				else
				{
					spawnOrigin = getRandomPosition(this.opposingSphereRadius, this.opposingSphereOrigin);
					if (!ProcSpawner.GetComponent<ProceduralSpawner>().withinArena(spawnOrigin))
					{
						Debug.Log(spawnOrigin);
						Debug.Log(opposingSphereOrigin);
						Debug.Log(opposingSphereRadius);
					}
				}
				Transform parent = this.transform.Find("Enemies");
				GameObject newEnemy = Instantiate(this.prefabToSpawn, spawnOrigin, Random.rotation, parent);
				GameObject enemyBody = newEnemy.transform.Find("Avatar").gameObject;
				GameManager.AddEnemy(enemyBody.GetInstanceID(), enemyBody);
				this.ProcSpawner.GetComponent<ProceduralSpawner>().enemyCount += 1;
			}
			
			// Now wait for the time between spawns to complete
			yield return new WaitForSeconds(this.timeBetweenSpawns);
		}
	}
}
