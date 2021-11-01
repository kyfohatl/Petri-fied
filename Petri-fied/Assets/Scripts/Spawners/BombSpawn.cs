using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombSpawn : SpawnerController
{
	// Treasure / Prefab at the centre of the bombs
	public GameObject SuperFoodAtCore;
	public GameObject PowerAtCore;
	public float probabilityPow = 0.2f;
	
	// Class parameters for food bombs
	public int innerSphereCount = 20;
	public int outerSphereCount = 40;
	public float innerRadius = 4f;
	public float outerRadius = 8f;
	public int pointsPerLayer = 8;
	public int numArmLayers = 3;
	public int totalFoodCount;
	
	// Called on start up of game
	void Awake()
	{
		this.ProcSpawner = GameObject.FindWithTag("Spawner");
		int armsFoodCount = this.numArmLayers * this.pointsPerLayer;
		this.totalFoodCount = this.innerSphereCount + this.outerSphereCount + armsFoodCount; // 60 + (8 *3)
		int foodLimit = GetComponent<FoodSpawn>().initialMaximum;
		this.spawnLimit = foodLimit - this.ProcSpawner.GetComponent<ProceduralSpawner>().foodCount;
	}
	
	// Start is called before the first frame update
	void Start()
	{
		
	}
	
	// If inspector makes changes
	void OnValidate()
	{
		int armsFoodCount = this.numArmLayers * this.pointsPerLayer;
		this.totalFoodCount = this.innerSphereCount + this.outerSphereCount + armsFoodCount; // 60 + (8 *3)
	}
	
	// Function to generate food bomb
	public IEnumerator GenerateBomb()
	{
		// Initial wait before first spawn
		yield return new WaitForSeconds(this.timeBetweenSpawns);
		while (this.enabled)
		{
			int spawnCount = Random.Range(this.spawnMin, this.spawnMax + 1);
			int foodLimit = GetComponent<FoodSpawn>().initialMaximum;
			for (int spawnCycle = 0; spawnCycle < this.spawnMax; spawnCycle++)
			{
				// Is there enough spawn space to build the bomb?
				this.spawnLimit = foodLimit - this.ProcSpawner.GetComponent<ProceduralSpawner>().foodCount;
				if (this.totalFoodCount >= this.spawnLimit)
				{
					break;
				}
				// Each layer of the bomb
				Vector3[] innerSphere = pointsOnSphere(this.innerSphereCount, this.innerRadius);
				Vector3[] outerSphere = pointsOnSphere(this.outerSphereCount, this.outerRadius);
				Vector3[] allPolePoints = new Vector3[this.totalFoodCount];
				float offset = this.outerRadius / 2f;
				
				for (int i = 0; i < this.numArmLayers; i++)
				{
					// Arms extending out of axis
					Vector3[] curLayerPoles = pointsOnSphere(this.pointsPerLayer, this.outerRadius + (i + 1) * offset);
					for (int j = 0; j < this.pointsPerLayer; j++)
					{
						int layerIncrement = i * this.pointsPerLayer;
						allPolePoints[j + layerIncrement] = curLayerPoles[j];
					}
				}
				// All arrays of points combined
				List<Vector3[]> allArrays = new List<Vector3[]>() {innerSphere, outerSphere, allPolePoints};
				Vector3 spawnOrigin = getRandomPosition();
				
				// Instantiates the core object in the sphere
				GameObject newBombCore = null;
				if (Random.value < this.probabilityPow) // Power Up (magnet)
				{
					Transform parent = this.transform.Find("PowerUps");
					newBombCore = Instantiate(this.PowerAtCore, spawnOrigin, Random.rotation, parent);
					newBombCore.GetComponent<PowerUpManager>().setType(1); // 1: magnet
					newBombCore.GetComponent<PowerUpMovement>().OrbitSpeed = 0f;
					GameManager.AddPowerUp(newBombCore.GetInstanceID(), newBombCore);
					this.ProcSpawner.GetComponent<ProceduralSpawner>().powerUpCount += 1;
				}
				else // SuperFood
				{
					Transform parent = this.transform.Find("Supers");
					newBombCore = Instantiate(this.SuperFoodAtCore, spawnOrigin, Random.rotation, parent);
					GameManager.AddFood(newBombCore.GetInstanceID(), newBombCore);
					GameManager.AddSuperFood(newBombCore.GetInstanceID(), newBombCore);
					this.ProcSpawner.GetComponent<ProceduralSpawner>().superFoodCount += 1;
				}
				
				// Now loop through each orbital food pellet around the core
				foreach (var subArray in allArrays)
				{
					foreach (var point in subArray)
					{
						GameObject newFood = GetComponent<FoodSpawn>().GetInactiveFood();
						if (newFood == null)
						{
							break;
						}
						if (newFood.activeInHierarchy)
						{
							Debug.Log("trying to use already active food");
						}
						
						newFood.transform.position = spawnOrigin + point;
						
						// Finally make food active and add to game manager dictionary
						newFood.SetActive(true);
						GameManager.AddFood(newFood.GetInstanceID(), newFood);
						this.ProcSpawner.GetComponent<ProceduralSpawner>().foodCount += 1;
					}
				}
			}
			
			// Now wait for the time between spawns to complete
			yield return new WaitForSeconds(this.timeBetweenSpawns);
		}
	}
	
	// Function to generate evenly distributed points across a spehere surface
	public Vector3[] pointsOnSphere(int numPoints, float radius)
	{
		// Uses fibonacci approximation method
		Vector3[] points = new Vector3[numPoints];
		float increment = Mathf.PI * (3f - Mathf.Sqrt(5f));
		float offset = 2f / numPoints;
		float x = 0;
		float y = 0;
		float z = 0;
		float r = 0;
		float phi = 0;
		
		// Generate each point and replace empty value in array
		for (int i = 0; i < numPoints; i++)
		{
			y = i * offset - 1f + (offset / 2f);
			r = Mathf.Sqrt(1f - y * y);
			phi = i * increment;
			x = Mathf.Cos(phi) * r;
			z = Mathf.Sin(phi) * r;
			
			points[i] = new Vector3(x, y, z) * radius;
		}
		return points;
	}
	
	// Getter function for total food count spawned by this script
	public int getTotalFoodCount()
	{
		return this.totalFoodCount;
	}
}
