using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodLife : MonoBehaviour
{
	// Time of instantiation and time before self deletion
	[SerializeField] private float HalfLifeSeconds = 120f; // time in seconds for one half of food to destroy
	
	// Start is called on first frame
	void Start()
	{
		StartCoroutine("Decompose");
	}
	
	// Update is called every frame
	void Update()
	{
		this.transform.Rotate(11f * Time.deltaTime, 0f, 7f * Time.deltaTime);
	}
	
	// Coroutine function to destroy 50% of objects in specified time frame
	IEnumerator Decompose()
	{
		yield return new WaitForSeconds (this.HalfLifeSeconds);
		if (Random.value > 0.5)
		{
			GameManager.RemoveFood(this.gameObject.GetInstanceID());
			if (this.gameObject.tag == "SuperFood")
			{
				GameManager.RemoveSuperFood(this.gameObject.GetInstanceID());
			}
			Destroy(this.gameObject);
		}
		else
		{
			StartCoroutine("Decompose"); // restart coroutine;
		}
	}
}
