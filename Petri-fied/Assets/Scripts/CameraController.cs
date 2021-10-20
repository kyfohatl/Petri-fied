using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
	// The main camera
	public Camera cam;
	// The player transform
	public Transform playerPos;
	// The arena dimensions
	private float arenaRadius;
	private float arenaHeight;
	private Vector3 arenaOrigin;
	
	// The camera orbit distance from the player character
	[SerializeField] private float orbitDistanceMult = 10f;
	// Influences the sensitivity of camera movement
	[SerializeField] private float cameraSensitivityMultiplier = 1f;
	
	// The current horizontal rotation of the camera, in degrees, around the player
	private float curHorizontalRotation = 0f;
	// The current vertical rotation of the camera, in degrees, around the player
	private float curVerticalRotation = 0f;
	// The current orbit distance of the camera
	private float curOrbitDistance;
	// The orbit distance the camera is aiming to get to
	private float goalOrbitDistance;
	[SerializeField] private float orbitChangeThreshold = 0.15625f;
	private bool atGoalOrbit;
	
	// Booleans to track inversions
	public bool invertX = false;
	public bool invertY = false;
	
	// Start is called before the first frame update
	void Start()
	{
		// Set ant event on GameEvents to change camera zoom upon player scale change
		GameEvents.instance.onPlayerRadiusChange += onPlayerRadiusChange;
		getArenaDimensions();
		this.curOrbitDistance = this.orbitDistanceMult * this.playerPos.localScale.x;
		this.goalOrbitDistance = this.curOrbitDistance;
		this.atGoalOrbit = true;
	}
	
	// Update is called once per frame
	void Update()
	{
		// Get movement input
		float hMouseInput = Input.GetAxisRaw("Mouse X");
		float vMouseInput = Input.GetAxisRaw("Mouse Y");
		float hKeyboardInput = Input.GetAxisRaw("Horizontal Look");
		float vKeyboardInput = Input.GetAxisRaw("Vertical Look");
		
		// Convert mouse/keyboard input into parsable data
		float hMovementInput = 0;
		float vMovementInput = 0;
		if (Mathf.Abs(hKeyboardInput) > 0) // some amount of left/right key
		{
			hMovementInput = hKeyboardInput;
		}
		else
		{
			hMovementInput = hMouseInput;
		}
		if (Mathf.Abs(vKeyboardInput) > 0) // some amount of up/down key
		{
			vMovementInput = vKeyboardInput;
		}
		else
		{
			vMovementInput = vMouseInput;
		}
		
		// Check/apply camera inversions
		if (this.invertX)
		{
			hMovementInput *= -1;
		}
		if (this.invertY)
		{
			vMovementInput *= -1;
		}
		
		// Increment horizontal and vertical rotation angles using input
		this.curHorizontalRotation = incrementAngle(curHorizontalRotation, hMovementInput);
		this.curVerticalRotation = incrementAngle(curVerticalRotation, vMovementInput);
		
		Vector3 newPosOffset = new Vector3(
		this.curOrbitDistance * Mathf.Cos(curHorizontalRotation) * Mathf.Sin(curVerticalRotation),
		this.curOrbitDistance * Mathf.Cos(curVerticalRotation),
		this.curOrbitDistance * Mathf.Sin(curHorizontalRotation) * Mathf.Sin(curVerticalRotation)
		);
		
		// Test the new camera position is within arena and if so, move it appropriately
		Vector3 newCameraPos = this.playerPos.position + newPosOffset;
		
		if (!withinArena(newCameraPos))
		{
			RaycastHit hit;
			if (Physics.Linecast(this.playerPos.position, newCameraPos, out hit))
			{
				Vector3 directionToPlayer = (this.playerPos.position - hit.point).normalized;
				newCameraPos = hit.point + directionToPlayer * 0.01f;
				this.goalOrbitDistance = Vector3.Distance(newCameraPos, this.playerPos.position);
			}
		}
		else
		{
			this.goalOrbitDistance = this.orbitDistanceMult * this.playerPos.localScale.x;
		}
		cam.transform.position = Vector3.Slerp(cam.transform.position, newCameraPos, 0.5f);
		
		// Camera scaling
		float orbitRatio = Mathf.Abs(this.goalOrbitDistance - this.curOrbitDistance) / this.goalOrbitDistance;
		if (orbitRatio > orbitChangeThreshold && this.atGoalOrbit)
		{
			this.atGoalOrbit = false;
		}
		if (!this.atGoalOrbit)
		{
			// If the camera is not at the goal orbit distance, interpolate orbit distance to desired
			this.curOrbitDistance = Mathf.Lerp(this.curOrbitDistance, this.goalOrbitDistance, 0.01f);
			if (Mathf.Abs(this.goalOrbitDistance - this.curOrbitDistance) < 0.01f)
			{
				this.atGoalOrbit = true;
			}
		}
	}
	
	// Late Update is called at the end of every frame
	private void LateUpdate()
	{
		// Always make sure the camera is looking at the player
		cam.transform.LookAt(this.playerPos);
	}
	
	// Increments the given angle by the given amount, taking into account framerate, and returns the result
	private float incrementAngle(float angle, float amount)
	{
		return (angle + amount * this.cameraSensitivityMultiplier * Time.deltaTime) % (2 * Mathf.PI);
	}
	
	// Changes the camera distance to the player to match the new given player radius
	private void onPlayerRadiusChange(float radius)
	{
		DepthOfField dof;
		PostProcessVolume postProcessor = GameObject.Find("PostProcessingGO").GetComponent<PostProcessVolume>();
		postProcessor.profile.TryGetSettings(out dof);
		dof.focusDistance.value = 6f * radius;

		this.goalOrbitDistance = this.orbitDistanceMult * radius;
	}
	
	// Returns the input with the grater magnitude
	private float getMaxInputVal(float input1, float input2)
	{
		if (Mathf.Abs(input1) > Mathf.Abs(input2))
		{
			return input1;
		}
		
		return input2;
	}
	
	// Function to determine spawner parameters given arena dimensions
	void getArenaDimensions()
	{
		GameObject arena =  GameObject.FindGameObjectWithTag("Arena");
		this.arenaRadius = arena.GetComponent<ArenaSize>().ArenaRadius;
		this.arenaHeight = arena.GetComponent<ArenaSize>().ArenaHeight;
		this.arenaOrigin = arena.gameObject.transform.position;
	}
	
	// Function to test if coordinate is within arena dimensions
	public bool withinArena(Vector3 testPoint)
	{
		float dx = Mathf.Abs(testPoint.x - this.arenaOrigin.x);
		float dy = Mathf.Abs(testPoint.y - this.arenaOrigin.y);
		float dz = Mathf.Abs(testPoint.z - this.arenaOrigin.z);
		
		if (dy > this.arenaHeight / 2f)
		{
			return false;
		}
		if (dx + dz <= this.arenaRadius)
		{
			return true;
		}
		if (dx > this.arenaRadius || dz > this.arenaRadius)
		{
			return false;
		}
		if (dx * dx + dz * dz < this.arenaRadius * this.arenaRadius)
		{
			return true;
		}
		
		// If here, must be outside arena
		return false;
	}
}
