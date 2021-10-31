using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralSpawner : MonoBehaviour
{
	// Instantiate this object
	public static ProceduralSpawner instance;
	
	// Track the player and arena to get relevant statistics
	public GameObject Player;
	public GameObject Arena;
	private float arenaSpawnRadius;
	private Vector3 arenaSpawnOrigin;
	
	// Each type of spawner
	private FoodSpawn FoodSpawner;
	private SuperFoodSpawn SuperFoodSpawner;
	private BombSpawn BombSpawner;
	private PowerUpSpawn PowerUpSpawner;
	private EnemySpawn EnemySpawner;
	
	// Counts of each active entity type in the scene
	public int foodCount = 0;
	public int superFoodCount = 0;
	public int enemyCount = 0;
	public int powerUpCount = 0;
	
	// Average position of each entity type (except food which can keep spawning everywhere equally)
	private Vector3 averageSuperFoodPosition;
	private Vector3 averageEnemyPosition;
	private Vector3 averagePowerUpPosition;
	
	// Tracker to scale to player's log10 score
	[SerializeField] private float playerLog10ScaleFactor;
	
	// Call on start-up of game
	private void Awake()
	{
		instance = this;
		DontDestroyOnLoad(this.gameObject);
	}
	
	// Start is called before the first frame update
	void Start()
	{
		this.Player = GameObject.FindWithTag("Player");
		this.Arena = GameObject.FindWithTag("Arena");
		GetInitialArenaDimensions();
		ScaleArena(); // this function calls
		EstablishSpawners();
		BeginSpawners();
	}
	
	// Update is called once per frame
	void Update()
	{
		ScaleArena();
	}
	
	// Function to get all attached sub-spawners
	void EstablishSpawners()
	{
		this.FoodSpawner = GetComponent<FoodSpawn>();
		this.SuperFoodSpawner = GetComponent<SuperFoodSpawn>();
		this.BombSpawner = GetComponent<BombSpawn>();
		this.PowerUpSpawner = GetComponent<PowerUpSpawn>();
		this.EnemySpawner = GetComponent<EnemySpawn>();
	}
	
	// Start the spawners
	void BeginSpawners()
	{
		StartCoroutine(GetComponent<FoodSpawn>().GenerateFood());
	}
	
	// Function to determine spawner parameters given arena dimensions
	void GetInitialArenaDimensions()
	{
		this.arenaSpawnRadius = this.Arena.GetComponent<ArenaSize>().ArenaRadius;
		this.arenaSpawnOrigin = this.Arena.gameObject.transform.position;
	}
	
	// Function to resize arena based on player score
	void ScaleArena()
	{
		float currentArenaRadius = this.Arena.GetComponent<ArenaSize>().ArenaRadius;
		DeterminePlayerScaleFactor();
		this.arenaSpawnRadius = currentArenaRadius * this.playerLog10ScaleFactor;
		this.Arena.GetComponent<TeleportSphere>().UpdateTeleportDiameter();
	}
	
	// Function to determine the Player Scale Factor
	void DeterminePlayerScaleFactor()
	{
		float playerScore = this.Player.GetComponent<IntelligentAgent>().getScore();
		float log10Score = Mathf.Log(playerScore, 10f);
		this.playerLog10ScaleFactor = Mathf.Max(log10Score, 1f);
	}
	
	// Function to test if coordinate is within arena spawn sphere
	public bool withinArena(Vector3 testPoint)
	{
		float distSqrdToTestPoint = (testPoint - this.arenaSpawnOrigin).sqrMagnitude;
		bool inSphere = (distSqrdToTestPoint < this.arenaSpawnRadius * this.arenaSpawnRadius);
		return inSphere;
	}
	public bool withinSphere(Vector3 testPoint, Vector3 sphereOrigin, float sphereRadius)
	{
		float distSqrdToTestPoint = (testPoint - sphereOrigin).sqrMagnitude;
		bool inSphere = (distSqrdToTestPoint < sphereRadius * sphereRadius);
		return inSphere;
	}
	
	// Getter function for player log 10 scale factor
	public float getPlayerLog10ScaleFactor()
	{
		return this.playerLog10ScaleFactor;
	}
}
