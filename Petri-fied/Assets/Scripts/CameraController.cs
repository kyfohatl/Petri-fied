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
  [SerializeField] private float cameraSensitivityMultiplier = 1f;

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
  private float previousFogStartDistance;
  private float previousFogEndDistance;
  private bool orbitUpdateTriggered = false;

  // The arena dimensions and boundary collision trackers
  private float arenaRadius;
  private float arenaHeight;
  private Vector3 arenaOrigin;
  private bool boundaryCollision = false;
  
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
    this.previousOrbitDistance = this.curOrbitDistance;
    this.goalOrbitDistance = this.orbitDistanceMult * this.playerRadius;
    this.orbitUpdateStartTime = Time.time;
    if (!GameManager.inst.gameOver)
    {
      InitialiseCameraPosition();
    }

	// Reset fog parameters to match lock-on radius distance
	updateFog();
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
	  float orbitRatio = this.curOrbitDistance / this.goalOrbitDistance;
	  float newFogEndDistance = orbitRatio * this.Player.GetComponent<IntelligentAgent>().getLockOnRadius();
	  
	  RenderSettings.fogStartDistance = Mathf.Lerp(this.previousFogStartDistance, newFogStartDistance, t);
	  RenderSettings.fogEndDistance = Mathf.Lerp(this.previousFogEndDistance, newFogEndDistance, t);
	  GetComponent<Camera>().farClipPlane = newFogEndDistance; // if we abandon the arena
	  
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
    if (this.boundaryCollision)
    {
      orbit = this.goalOrbitDistance;
    }
    Vector3 newCameraPos = newCameraPosition(orbit);
    UpdateCameraPosition(newCameraPos);

    transform.LookAt(this.Player.transform); // ensure the camera is looking at the player, respect to Vector3.up
  }

  // Function to update camera transform position
  void UpdateCameraPosition(Vector3 newCameraPos)
  {
	  this.transform.position = newCameraPos;
	  return;
    // Initialize outpit and start by testing if the new camera position is outside arena
    Vector3 finalPosition = newCameraPos;
    if (!withinArena(newCameraPos))
    {
      RaycastHit hit;
      if (Physics.Linecast(this.Player.transform.position, newCameraPos, out hit))
      {
        if (hit.collider.tag != "Arena")
        {
			return; // don't intersect with any bodies other than the arena
        }

        this.boundaryCollision = true;
        Vector3 directionToPlayer = (this.Player.transform.position - hit.point).normalized;
        finalPosition = hit.point + directionToPlayer * 0.25f; // move camera slightly ahead of boundary hit
        this.curOrbitDistance = Vector3.Distance(finalPosition, this.Player.transform.position);
      }
    }
    else
    {
      if (this.boundaryCollision)
      {
        // Reset the boundary collision flag if current cam distance is within tolerance
		this.curOrbitDistance = Vector3.Distance(this.Player.transform.position, this.transform.position);
        if (Mathf.Abs(this.curOrbitDistance - this.goalOrbitDistance) < 0.02f)
        {
          this.curOrbitDistance = this.goalOrbitDistance;
          this.boundaryCollision = false;
        }
      }
    }

    // Finally, set the camera position to the new position
    if (!this.boundaryCollision)
    {
		this.transform.position = finalPosition;
    }
    else
    {
      float boundarySmooth = 10f;
	  this.transform.position = Vector3.Lerp(this.transform.position, finalPosition, Time.deltaTime * boundarySmooth);
	  this.curOrbitDistance = Vector3.Distance(this.Player.transform.position, this.transform.position);
    }
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

  // Function to test if coordinate is within arena dimensions
  private bool withinArena(Vector3 testPoint)
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
	this.previousFogStartDistance = RenderSettings.fogStartDistance;
	this.previousFogEndDistance = RenderSettings.fogEndDistance;
  }

  // Increments the given angle by the given amount, taking into account framerate, and returns the result
  private float incrementAngle(float angle, float amount)
  {
    float orbitRatio = this.curOrbitDistance / this.goalOrbitDistance;
    float boundarySmooth = Mathf.Max(1f, this.cameraSensitivityMultiplier * orbitRatio);
    float newAngle = (angle + amount * boundarySmooth * Time.deltaTime) % (2f * Mathf.PI);
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
    this.arenaHeight = arena.GetComponent<ArenaSize>().ArenaHeight;
    this.arenaOrigin = arena.gameObject.transform.position;
  }
  
  // Function to update the fog render distance
  void updateFog()
  {
	  if (RenderSettings.fog) // is fog enabled
	  {
		  float lockOnRadius = this.Player.GetComponent<IntelligentAgent>().getLockOnRadius();
		  RenderSettings.fogStartDistance = this.goalOrbitDistance;
		  RenderSettings.fogEndDistance = RenderSettings.fogStartDistance + lockOnRadius;
		  GetComponent<Camera>().farClipPlane = RenderSettings.fogEndDistance; // if we abandon the arena
	  }
  }
  
  // Getter function for current goal orbit radius
  public float getGoalOrbitDistance()
  {
	  return this.goalOrbitDistance;
  }
}
