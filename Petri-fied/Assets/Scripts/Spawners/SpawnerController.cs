using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{
	// Prefab of the desired spawnable object
	public GameObject prefabToSpawn;
	private GameObject Arena;
	
	// Spawn rate and constraints
	public float timeBetweenSpawns = 5f; // time in seconds between spawns
	public int spawnMin = 1; // lower bound spawns per generation
	public int spawnMax = 1; // upper bound spawns per generation
	public int spawnLimit = 20; // maximum spawns allowed
	
	protected float timer = 0f; // clock to track time between generations
	
	// Arena and spawner dimensions
	protected float arenaRadius;
	protected Vector3 arenaOrigin;
	
	// Function to determine spawner parameters given arena dimensions
	void getArenaDimensions()
	{
		if (this.Arena == null)
		{
			this.Arena = GameObject.FindWithTag("Arena");
		}
		this.arenaRadius = this.Arena.GetComponent<ArenaSize>().ArenaRadius;
		this.arenaOrigin = this.Arena.transform.position;
	}
	
	// Function to determine how many new objects to spawn in this cycle
	public int NewSpawnCount(int currentCount)
	{
		int spawnCount = Random.Range(this.spawnMin, this.spawnMax + 1);
		if (currentCount + spawnCount > this.spawnLimit)
		{
			spawnCount = this.spawnLimit - this.transform.childCount;
		}
		return spawnCount;
	}
	
	// Function to instantiate new prefab objects into the world
	public GameObject Generate()
	{
		// Determine spawn position
		Vector3 Target = getRandomPosition();
		// Instantiates the newly spawned object and sets as child of spawner
		GameObject spawned = Instantiate(this.prefabToSpawn, Target, Random.rotation, transform);
		return spawned;
	}
	public GameObject Generate(Vector3 spawnPosition)
	{
		GameObject spawned = Instantiate(this.prefabToSpawn, spawnPosition, Random.rotation, transform);
		return spawned;
	}
	
	// Function to generate a random position somewhere inside the arena dimensions
	public Vector3 getRandomPosition() // generic
	{
		getArenaDimensions();
		return Random.insideUnitSphere * this.arenaRadius + this.arenaOrigin;
	}
	public Vector3 getRandomPosition(float inRadius) // for specified radius
	{
		getArenaDimensions();
		return Random.insideUnitSphere * inRadius + this.arenaOrigin;
	}
	public Vector3 getRandomPosition(float inRadius, Vector3 originPoint) // for specified radius and origin
	{
		getArenaDimensions();
		return Random.insideUnitSphere * inRadius + originPoint;
	}
}
