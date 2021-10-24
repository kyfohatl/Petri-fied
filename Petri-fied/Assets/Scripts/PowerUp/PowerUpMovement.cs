using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpMovement : MonoBehaviour
{
	public float orbitSpeed = 2f; // X degrees
	private Vector3 arenaOrigin;
	
    // Start is called before the first frame update
    void Start()
    {
		GameObject arena = GameObject.FindGameObjectWithTag("Arena");
		this.arenaOrigin = arena.gameObject.transform.position;
    }

    // Update is called once per frame
	private void Update()
	{
		// Spin the object around the target at X degrees/second.
		transform.RotateAround(arenaOrigin, Vector3.up, this.orbitSpeed * Time.deltaTime);
	}
}
