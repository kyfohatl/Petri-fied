using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpMovement : MonoBehaviour
{
	public float OrbitSpeed = 1f; // X degrees per second, can be huge
	public bool OrbitClockwise = true; // -/0/+
	public bool Moving = false;
	private Vector3 arenaOrigin;
	
    // Start is called before the first frame update
    void Start()
    {
		// Determine arena origin
		GameObject arena = GameObject.FindWithTag("Arena");
		this.arenaOrigin = arena.gameObject.transform.position;
		// Set the type of movement and direction;
		if (Random.value > 0.5f)
		{
			this.Moving = true;
			this.OrbitClockwise = Random.value > 0.5f; // easy boolean test
		}
    }

    // Update is called once per frame
	private void Update()
	{
		if (this.Moving)
			{
			// Spin the object around the target at X degrees/second.
			if (this.OrbitClockwise)
			{
				transform.RotateAround(arenaOrigin, Vector3.up, this.OrbitSpeed * Time.deltaTime);
			}
			else
			{
				transform.RotateAround(arenaOrigin, -Vector3.up, this.OrbitSpeed * Time.deltaTime);
			}
		}
	}
}
