using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodHalfLife : MonoBehaviour
{
	// Time of instantiation and time before self deletion
	[SerializeField] private float HalfLifeSeconds = 120f; // time in seconds for one half of food to destroy
	
	// Update is called once per frame
	void Start()
	{
		StartCoroutine("Decompose");
	}
	
	IEnumerator Decompose()
	{
		yield return new WaitForSeconds (this.HalfLifeSeconds);
		if (Random.value > 0.5)
		{
			GameManager.RemoveFood(this.gameObject.GetInstanceID());
			Destroy(this.gameObject);
		}
		else
		{
			StartCoroutine("Decompose"); // restart coroutine;
		}
	}
}
