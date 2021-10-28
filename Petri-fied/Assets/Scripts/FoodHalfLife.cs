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
	
	void Update()
	{
		transform.Rotate(30f * Time.deltaTime, 0f, 5f * Time.deltaTime);
	}
	
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
