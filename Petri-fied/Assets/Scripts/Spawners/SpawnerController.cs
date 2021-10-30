using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{
	// Prefab of the desired spawnable object
	public GameObject prefabToSpawn;
	
	// Spawn rate and constraints
	public float timeBetweenSpawns = 5f; // time in seconds between spawns
	public int spawnMin = 1; // lower bound spawns per generation
	public int spawnMax = 1; // upper bound spawns per generation
	public int spawnLimit = 20; // maximum spawns allowed
	
	protected float timer = 0f; // clock to track time between generations
	
	// Arena and spawner dimensions
	protected float arenaRadius;
	protected Vector3 arenaOrigin;
	
	// Start is called before the first frame update
	void Start()
	{
		// Determine current arena dimensions (useful if arena scales in future)
		getArenaDimensions();
	}
	
	// Function to determine spawner parameters given arena dimensions
	void getArenaDimensions()
	{
		GameObject arena =  GameObject.FindGameObjectWithTag("Arena");
		this.arenaRadius = arena.GetComponent<ArenaSize>().ArenaRadius;
		this.arenaOrigin = arena.gameObject.transform.position;
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
		// Instantiates the newly spawned object and sets as child of spawner
		GameObject spawned = Instantiate(this.prefabToSpawn, spawnPosition, Random.rotation, transform);
		return spawned;
	}
	
	// Function to generate a random position somewhere inside the arena dimensions
	public Vector3 getRandomPosition() // generic
	{
		return Random.insideUnitSphere * this.arenaRadius + this.arenaOrigin;
	}
	public Vector3 getRandomPosition(float inRadius) // for specified radius
	{
		return Random.insideUnitSphere * inRadius + this.arenaOrigin;
	}
	public Vector3 getRandomPosition(float inRadius, Vector3 originPoint) // for specified radius and origin
	{
		return Random.insideUnitSphere * inRadius + originPoint;
	}
}
