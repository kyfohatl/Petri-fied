using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
	// The target the camera follows
	public GameObject Player;
	private float playerRadius;
	
	// The camera orbit multiplier / sensitivity with respect to the player
	[SerializeField] private float orbitDistanceMult = 10f;
	[SerializeField] public float cameraSensitivityMultiplier = 1f;
	
	// Booleans to track inversions
	public bool invertX = false;
	public bool invertY = false;
	public bool enableFollowMouse = true;
	
	// The current vertical/horizontal rotation of the camera, in radians, around the player
	private float curHorizontalRotation = -Mathf.PI / 2f;
	private float curVerticalRotation = Mathf.PI / 2f;
	private float curOrbitDistance;
	private float goalOrbitDistance;
	
	// Orbit update trackets
	private float orbitUpdateDuration = 2f; // time to update to new orbit, default: 2 seconds
	private float orbitUpdateStartTime;
	private float previousOrbitDistance;
	private float previousFogEndDistance;
	private bool orbitUpdateTriggered = true;
	
	// The arena dimensions and boundary collision trackers
	private float arenaRadius;
	private Vector3 arenaOrigin;
	
	// Called on start-up
	void Awake()
	{
		// Prevent camera from being destroyed between scenes.
		DontDestroyOnLoad(this.gameObject);
	}
	
	// Start is called before the first frame update
	void Start()
	{
		// Get initial game conditions and player
		this.Player = GameObject.FindGameObjectWithTag("Player");
		this.playerRadius = Player.GetComponent<IntelligentAgent>().getRadius();
		getArenaDimensions();
		
		// Set an event on GameEvents to change camera zoom upon player scale change
		GameEvents.instance.onPlayerRadiusChange += onPlayerRadiusChange;
		
		this.curHorizontalRotation = -Mathf.PI / 2f;
		this.curVerticalRotation = Mathf.PI / 2f;
		
		this.curOrbitDistance = Vector3.Distance(transform.position, this.Player.transform.position);
		this.goalOrbitDistance = this.orbitDistanceMult * this.playerRadius;
		this.previousOrbitDistance = this.curOrbitDistance;
		this.orbitUpdateStartTime = Time.time;
		
		if (!GameManager.inst.gameOver)
		{
			InitialiseCameraPosition();
		}
		
		// Reset fog and far clipping plane parameters to match lock-on radius distance + cam orbit distance
		UpdateFog();
	}
	
	// Set the camera to the player.
	public void InitialiseCameraPosition()
	{
		UpdateCameraPosition(newCameraPosition(this.goalOrbitDistance));
	}
	
	// Update is called once per frame
	void Update()
	{
		// Disable camera updates if game isn't running.
		if (GameManager.get().gameOver)
		{
			return;
		}
		
		// Get movement input
		float hMouseInput = 0f;
		float vMouseInput = 0f;
		if (this.enableFollowMouse)
		{
			hMouseInput = Input.GetAxisRaw("Mouse X");
			vMouseInput = Input.GetAxisRaw("Mouse Y");
		}
		float hKeyboardInput = Input.GetAxisRaw("Horizontal Look");
		float vKeyboardInput = Input.GetAxisRaw("Vertical Look");
		
		// Convert mouse / keyboard input into parsable data
		float hMovementInput = 0f;
		float vMovementInput = 0f;
		hMovementInput = getMaxInputVal(hMouseInput, hKeyboardInput);
		vMovementInput = getMaxInputVal(vMouseInput, vKeyboardInput);
		
		// Check / apply camera inversions
		if (this.invertX)
		{
			hMovementInput *= -1;
		}
		if (this.invertY)
		{
			vMovementInput *= -1;
		}
		
		// Increment horizontal and vertical rotation angles using input
		if (hMovementInput != 0f)
		{
			this.curHorizontalRotation = incrementAngle(curHorizontalRotation, hMovementInput);
		}
		if (vMovementInput != 0f)
		{
			this.curVerticalRotation = Mathf.Clamp(incrementAngle(curVerticalRotation, vMovementInput), 0.174f, 2.967f); // [10deg, 170deg]
		}
		
		// Orbit re-scaling due to radius changes
		if (this.orbitUpdateTriggered)
		{
			// If the camera is not at the goal orbit distance, interpolate orbit distance to desired
			float t = (Time.time - this.orbitUpdateStartTime) / this.orbitUpdateDuration;
			this.curOrbitDistance = Mathf.Lerp(this.previousOrbitDistance, this.goalOrbitDistance, t);
			
			float newFogStartDistance = this.curOrbitDistance;
			float lockOnRadius = this.Player.GetComponent<IntelligentAgent>().getLockOnRadius();
			float newFogEndDistance = (newFogStartDistance + lockOnRadius);
			
			RenderSettings.fogStartDistance = newFogStartDistance;
			RenderSettings.fogEndDistance = Mathf.Lerp(this.previousFogEndDistance, newFogEndDistance, t);
			UpdateFarClippingPlane(1.01f * newFogEndDistance);
			
			if (t >= 1f)
			{
				this.orbitUpdateTriggered = false;
			}
		}
	}
	
	// Late Update is called at the end of every frame
	private void LateUpdate()
	{
		// Disable camera updates if game isn't running.
		if (GameManager.get().gameOver)
		{
			return;
		}
		
		// Camera orbits calculation and positioning
		float orbit = this.curOrbitDistance;
		if (orbit > this.goalOrbitDistance)
		{
			this.curOrbitDistance = this.goalOrbitDistance;
			orbit = this.goalOrbitDistance;
			UpdateFog();
		}
		Vector3 newCameraPos = newCameraPosition(orbit);
		UpdateCameraPosition(newCameraPos);
		
		transform.LookAt(this.Player.transform); // ensure the camera is looking at the player, respect to Vector3.up
	}
	
	// Function to update camera transform position
	void UpdateCameraPosition(Vector3 newCameraPos)
	{
		this.transform.position = newCameraPos;
	}
	
	// Function to determine new position of camera
	private Vector3 newCameraPosition(float inRadius)
	{
		// Determine desired position of camera given axis rotaions and current orbit radius
		Vector3 newPosOffset = new Vector3(
		inRadius * Mathf.Cos(this.curHorizontalRotation) * Mathf.Sin(this.curVerticalRotation),
		inRadius * Mathf.Cos(this.curVerticalRotation),
		inRadius * Mathf.Sin(this.curHorizontalRotation) * Mathf.Sin(this.curVerticalRotation));
		
		return this.Player.transform.position + newPosOffset;
	}
	
	// Changes the camera distance to the player to match the new given player radius
	private void onPlayerRadiusChange(float radius)
	{
		// Modify depth-of-field blurriness to adjust to new radius
		DepthOfField dof;
		PostProcessVolume postProcessor = GameObject.Find("PostProcessingGO").GetComponent<PostProcessVolume>();
		postProcessor.profile.TryGetSettings(out dof);
		dof.focusDistance.value = 6f * Mathf.Sqrt(radius);
		
		// Set new goal orbit as per radius change
		this.previousOrbitDistance = this.curOrbitDistance;
		this.playerRadius = radius;
		this.goalOrbitDistance = this.orbitDistanceMult * radius;
		this.orbitUpdateTriggered = true;
		this.orbitUpdateStartTime = Time.time;
		
		// Reset fog parameters to match lock-on radius distance
		this.previousFogEndDistance = RenderSettings.fogEndDistance;
	}
	
	// Increments the given angle by the given amount, taking into account framerate, and returns the result
	private float incrementAngle(float angle, float amount)
	{
		float newAngle = (angle + amount * this.cameraSensitivityMultiplier * Time.deltaTime) % (2f * Mathf.PI);
		return newAngle;
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
		GameObject arena = GameObject.FindGameObjectWithTag("Arena");
		this.arenaRadius = arena.GetComponent<ArenaSize>().ArenaRadius;
		this.arenaOrigin = arena.gameObject.transform.position;
	}
	
	// Function to update the fog render distance to align with player lock-on radius
	void UpdateFog()
	{
		if (RenderSettings.fog) // is fog enabled
		{
			float lockOnRadius = this.Player.GetComponent<IntelligentAgent>().getLockOnRadius();
			RenderSettings.fogStartDistance = this.goalOrbitDistance;
			float endDistance = this.goalOrbitDistance + lockOnRadius;
			RenderSettings.fogEndDistance = endDistance;
			UpdateFarClippingPlane(1.1f * endDistance);
		}
	}
	
	// Function to update the main camera far clipping plane to align with fog end render distance
	void UpdateFarClippingPlane(float newDistance)
	{
		GetComponent<Camera>().farClipPlane = newDistance;
	}
	
	// Getter function for current goal orbit radius
	public float getGoalOrbitDistance()
	{
		return this.goalOrbitDistance;
	}

  // Setter for cam invert x
  public void SetInvertX(bool enabled)
  {
    this.invertX = enabled;
  }

  // Setter for cam invert y
  public void SetInvertY(bool enabled)
  {
    this.invertY = enabled;
  }

  // Setter for cam follow mouse
  public void SetFollowMouse(bool enabled)
  {
    this.enableFollowMouse = enabled;
  }

  public void SetCameraSensitivity(float multiplier)
  {
    this.cameraSensitivityMultiplier = multiplier;
  }
}
