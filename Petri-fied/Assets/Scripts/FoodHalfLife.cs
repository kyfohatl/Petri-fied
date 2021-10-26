using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodHalfLife : MonoBehaviour
{
	// Time of instantiation and time before self deletion
	private float timeOfSpawn = 0f;
	[SerializeField] private float HalfLifeSeconds = 120f; // time in seconds for one half of food to destroy


    // Update is called once per frame
    void Update()
    {
		if (Time.time - this.timeOfSpawn >= this.HalfLifeSeconds)
		{
			if (Random.value > 0.5)
			{
				GameManager.RemoveFood(this.gameObject.GetInstanceID());
				Destroy(this.gameObject);
			}
			else
			{
				this.timeOfSpawn = Time.time;
			}
		}
    }
}
