using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombSpawn : SpawnerController
{
	// Treasure / Prefab at the centre of the bombs
	public GameObject prefabAtCore;
	
	// Class parameters for food bombs
	public int innerSphereCount = 15;
	public int outerSphereCount = 35;
	public float innerRadius = 3f;
	public float outerRadius = 6f;

    // Update is called once per frame
    void Update()
    {
		this.timer += Time.deltaTime;
		
		if (this.timer >= this.timeBetweenSpawns)
		{
			this.timer = 0f;
			int spawnCount = this.innerSphereCount + this.outerSphereCount;
			if (transform.childCount < this.spawnLimit - spawnCount)
			{
				generateFoodBomb();
			}
		}
    }
	
	// Function to generate food bomb
	public void generateFoodBomb()
	{
		// Each layer of the bomb
		Vector3[] innerSphere = pointsOnSphere(this.innerSphereCount, this.innerRadius);
		Vector3[] outerSphere = pointsOnSphere(this.outerSphereCount, this.outerRadius);
		float offset = this.outerRadius / 2f;
		// Arms extending out of axis
		Vector3[] innerPoles = pointsOnSphere(8, this.outerRadius + offset);
		Vector3[] midPoles = pointsOnSphere(8, this.outerRadius + 2 * offset);
		Vector3[] outerPoles = pointsOnSphere(8, this.outerRadius + 3 * offset);
		// All points arrays collated
		List<Vector3[]> allArrays = new List<Vector3[]>() {innerSphere,
														   outerSphere,
														   innerPoles,
														   midPoles,
														   outerPoles};
		// Position and restriction of bomb origin spawn location
		float spawnRadius = this.spawnRadius - (this.outerRadius + 3 * offset);
		float spawnHeight = this.spawnHeight - (this.outerRadius + 3 * offset);
		Vector3 spawnOrigin = getRandomPosition(spawnRadius, spawnHeight);
		
		// Instantiates the core object in the sphere
		GameObject newlyCreated = Instantiate(this.prefabAtCore, spawnOrigin, Random.rotation, transform);
		GameManager.AddFood(newlyCreated.GetInstanceID(), newlyCreated);
		
		foreach (var subArray in allArrays)
		{
			foreach (var point in subArray)
			{
				// Instantiates the newly spawned object and sets as child of spawner
				newlyCreated = Instantiate(this.prefabToSpawn, spawnOrigin + point, Random.rotation, transform);
				GameManager.AddFood(newlyCreated.GetInstanceID(), newlyCreated);
			}
		}
		
	}
	
	// Function to generate evenly distributed points across a spehere surface
	public Vector3[] pointsOnSphere(int numPoints, float radius)
	{
		// Uses fibonacci approximation method
		Vector3[] points = new Vector3[numPoints];
		float increment = Mathf.PI * (3 - Mathf.Sqrt(5));
		float offset = 2.0f / numPoints;
		float x = 0;
		float y = 0;
		float z = 0;
		float r = 0;
		float phi = 0;
		
		// Generate each point and replace empty value in array
		for (int i = 0; i < numPoints; i++)
		{
			y = i * offset - 1 + (offset / 2);
			r = Mathf.Sqrt(1 - y * y);
			phi = i * increment;
			x = Mathf.Cos(phi) * r;
			z = Mathf.Sin(phi) * r;
			
			points[i] = new Vector3(x, y, z) * radius;
		}
		return points;
	}
}
