using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn :  SpawnerController
{
	// Update is called once per frame
	void Update()
	{
		this.timer += Time.deltaTime;
		
		if (this.timer > this.timeBetweenSpawns && transform.childCount < this.spawnLimit)
		{
			this.timer = 0f;
			int spawnCount = Random.Range(spawnMin, spawnMax + 1);
			
			// Loop to spawn one or more powerUp objects per generation
			for (int i = 0; i < spawnCount; i++)
			{
				// Track the PowerUp spawned.
				GameObject newlyCreated = Generate();
				GameObject enemyBody = newlyCreated.transform.Find("Avatar").gameObject;
				GameManager.AddEnemy(enemyBody.GetInstanceID(), enemyBody);
			}
		}
	}
}
