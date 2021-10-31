using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSphere : MonoBehaviour
{
	// Player object for ease of retrieval
	private IntelligentAgent Player;
	
	// Teleport radius is relative to the arena spawn dimensions and plyer lock on radius
	public float TeleportDiameter = 300f;
	private float arenaRadius;
	private float playerLockOnRadius;
	
    // Start is called before the first frame update
    void Start()
    {
		// Get initial game conditions and player data
		this.Player = GameObject.FindGameObjectWithTag("Player").GetComponent<IntelligentAgent>();
		this.arenaRadius = GetComponent<ArenaSize>().ArenaRadius;
		UpdateTeleportDiameter();
    }
	
	// Update is called once per frame
	void Update()
	{
		float currentLockOnRadius = this.Player.getLockOnRadius();
		if (currentLockOnRadius > this.playerLockOnRadius)
		{
			UpdateTeleportDiameter();
		}
	}
	
	// If inspector changes value
	void OnValidate()
	{
		this.transform.localScale = this.TeleportDiameter * new Vector3(1f, 1f, 1f);
	}
	
	// Update the teleport diameter to reflect current dimensions
	public void UpdateTeleportDiameter()
	{
		float cameraOrbit = Camera.main.GetComponent<CameraController>().getGoalOrbitDistance();
		this.arenaRadius = GetComponent<ArenaSize>().ArenaRadius;
		this.TeleportDiameter = 2f * (this.arenaRadius + 2f * cameraOrbit);
		this.transform.localScale = this.TeleportDiameter * new Vector3(1f, 1f, 1f);
	}
	
	
	// Function called on collisions
	void OnTriggerExit(Collider other)
	{
		// Handle the player avatar having it's own hitbox
		if (other.gameObject.tag == "PlayerBody")
		{
			return; // do nothing, we only want to move the parent player object
		}
		
		// Now calculate where the hit was and where the antipole should be
		Vector3 hitPoint = other.transform.position;
		Vector3 antipolePoint = new Vector3(0f, 0f, 0f);
		antipolePoint += -1f * hitPoint;
		
		if (other.gameObject.tag == "Player")
		{
			// Character controller prevents teleportation, needs temporary disabling
			CharacterController cc = this.Player.GetComponent<CharacterController>();
			cc.enabled = false;
			this.Player.transform.position = antipolePoint;
			cc.enabled = true;
		}
		else if (other.gameObject.tag == "Enemy")
		{
			other.gameObject.transform.position = antipolePoint;
		}
		
		Debug.Log(other.name + " teleported from: " + hitPoint + " to: " + antipolePoint);
	}
}
