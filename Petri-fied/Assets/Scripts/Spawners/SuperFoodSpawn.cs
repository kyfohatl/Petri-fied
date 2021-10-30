using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperFoodSpawn : SpawnerController
{
	// Update is called once per frame
	void Update()
	{
		this.timer += Time.deltaTime;
		
		if (this.timer > this.timeBetweenSpawns && this.transform.childCount < this.spawnLimit)
		{
			this.timer = 0f;
			int spawnCount = Random.Range(this.spawnMin, this.spawnMax + 1);
			if (this.transform.childCount + spawnCount > this.spawnLimit)
			{
				spawnCount = this.spawnLimit - this.transform.childCount;
			}
			
			// Loop to spawn one or more food objects per generation
			for (int i = 0; i < spawnCount; i++)
			{
				// Track the object spawned.
				GameObject newlyCreated = Generate();
				GameManager.AddFood(newlyCreated.GetInstanceID(), newlyCreated);
				
				if (newlyCreated.tag == "SuperFood")
				{
					GameManager.AddSuperFood(newlyCreated.GetInstanceID(), newlyCreated);
				}
			}
		}
	}
}
