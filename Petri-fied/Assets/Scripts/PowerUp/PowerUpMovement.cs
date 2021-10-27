using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpMovement : MonoBehaviour
{
	public float OrbitSpeed = 1f; // X degrees per second, can be huge
	public bool OrbitClockwise = true; // -/0/+
	public bool RandomPath;
	private Vector3 randomDirection;
	private Vector3 arenaOrigin;
	
    // Start is called before the first frame update
    void Start()
    {
		// Determine arena origin
		GameObject arena = GameObject.Find("Arena");
		this.arenaOrigin = arena.gameObject.transform.position;

		// Set the type of movement and direction;
		this.OrbitSpeed = Random.value;
		this.OrbitClockwise = Random.value > 0.5f; // easy boolean test
		if (Random.value > 0.5f)
		{
			this.RandomPath = true;
			this.randomDirection = Random.onUnitSphere;
		}
    }

    // Update is called once per frame
	private void Update()
	{
		if (!this.RandomPath)
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
		else
		{
			// transform.Translate(this.randomDirection * Time.deltaTime * this.OrbitSpeed);
		}
	}
}
