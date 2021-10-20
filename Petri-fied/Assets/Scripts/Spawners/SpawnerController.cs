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
	public float spawnRadius = 50f;
	public float spawnHeight = 50f;
	private float arenaRadius;
	private float arenaHeight;
	private Vector3 arenaOrigin;
	
	// Start is called before the first frame update
	void Start()
	{
		getArenaDimensions();
		// Set spawner parameters
		this.spawnRadius = this.arenaRadius - 1f;
		this.spawnHeight = this.arenaHeight - 1f;
	}
	
	// Function to determine spawner parameters given arena dimensions
	void getArenaDimensions()
	{
		GameObject arena =  GameObject.FindGameObjectWithTag("Arena");
		this.arenaRadius = arena.GetComponent<ArenaSize>().ArenaRadius;
		this.arenaHeight = arena.GetComponent<ArenaSize>().ArenaHeight;
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
	
	// Function to generate a random position somewhere inside the spawner dimensions
	public Vector3 getRandomPosition()
	{
		Vector2 xz = Random.insideUnitCircle * this.spawnRadius;
		float y = Random.Range(this.arenaOrigin.z - this.spawnHeight / 2f, this.arenaOrigin.z + this.spawnHeight / 2f);
		Vector3 randomPos = new Vector3(this.arenaOrigin.x + xz.x, this.arenaOrigin.y + y, this.arenaOrigin.z + xz.y);
		
		return randomPos;
	}
	public Vector3 getRandomPosition(float inRadius, float inHeight)
	{
		Vector2 xz = Random.insideUnitCircle * inRadius;
		float y = Random.Range(this.arenaOrigin.z - inHeight / 2f, this.arenaOrigin.z + inHeight / 2f);
		Vector3 randomPos = new Vector3(this.arenaOrigin.x + xz.x, this.arenaOrigin.y + y, this.arenaOrigin.z + xz.y);
		
		return randomPos;
	}
}
