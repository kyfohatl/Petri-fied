using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(IsMoving))]
public class PlayerController : MonoBehaviour
{
	// Basic parameters
	[SerializeField] private CharacterController controller;
	[SerializeField] private float speedMultiplier = 10f;
	[SerializeField] private float acceleration = 2f;
	[SerializeField] private float turnSmooth = 0.05f;
	
	// Movement-type triggers (both should be avaliable in settings, can be used well together)
	public bool AutoFollowTarget = false; // will automatically fly at target whenever set
	public bool AutoFollowCursor = false; // will automatically fly forward in the turn direction of the cursor
	
	// Current forward direction of player
	private Vector3 curDir = new Vector3(0f, 0f, 0f);
	
	// Entity Manager (contains info of relevant entities)
	protected GameManager GameManager;
	
	// Start is called before the first frame update
	void Start()
	{
		this.GameManager = FindObjectOfType<GameManager>();
	}
	
	// Update is called once per frame
	void Update()
	{
		// Disable updates if maingame isn't running.
		if (GameManager.gameOver)
		{
			return;
		}
		
		// Calculate the target direction of movement
		Vector3 horizontalMoveDir = Camera.main.transform.right * Input.GetAxisRaw("Horizontal");
		Vector3 verticalMoveDir = Camera.main.transform.forward * Input.GetAxisRaw("Vertical");
		Vector3 hoverMoveDir = Camera.main.transform.up * Input.GetAxisRaw("Hover");
		Vector3 targetDir = (horizontalMoveDir + verticalMoveDir + hoverMoveDir);
		GameObject curTarget = GetComponent<Player>().getTarget();
		
		if (targetDir.magnitude >= 0.1f || (this.AutoFollowTarget && curTarget != null))
		{
			// Player is moving
			GetComponent<IsMoving>().isMoving = true;
			
			// Prioritise lock-on target first
			if (curTarget != null)
			{
				targetDir = (curTarget.transform.position - this.transform.position).normalized;
				this.curDir = targetDir;
			}
			else if (this.AutoFollowCursor)
			{
				// Rotate the model to face the direction of travel
				this.curDir = Vector3.Slerp(curDir, targetDir, acceleration * Time.deltaTime);
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), turnSmooth);
			}
			else
			{
				// Rotate the model to face the direction of travel
				this.curDir = Vector3.Slerp(curDir, targetDir, acceleration * Time.deltaTime);
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), turnSmooth);
			}
			
			// Move towards the current direction
			float geneticSpeedMultiplier = GetComponent<Player>().getSpeedMultiplier() * GetComponent<Player>().getPowerUpSpeedMultiplier();
			float finalSpeedMultiplier = this.speedMultiplier * geneticSpeedMultiplier; // this.speedMultiplier is an artifact from the character controller == 10f
			
			controller.Move(curDir * finalSpeedMultiplier * Time.deltaTime / transform.localScale.x);
		}
		else
		{
			// Player is not moving
			GetComponent<IsMoving>().isMoving = false;
		}
		
		// Lock-on feature using X to change to next closest visible enemy, locks-onto closest if no current target
		if (Input.GetKeyDown(KeyCode.X))
		{
			enemyLockOn();
		}
		
		// Lock-on feature using C to change to next closest visible power up, locks-onto closest if no current target
		if (Input.GetKeyDown(KeyCode.C))
		{
			powerUpLockOn();
		}
		
		// Lock-on feature using F to change to next closest visible food capsule, locks-onto closest if no current target
		if (Input.GetKeyDown(KeyCode.F))
		{
			foodLockOn();
		}
		
		// Lock-on feature using left mouse click
		if (Input.GetMouseButtonDown(0))
		{
			mouseLockOn();
		}
		
		// Lock-on feature using left mouse click
		if (Input.GetKeyDown(KeyCode.Space))
		{
			// this.AutoFollowCursor = !this.AutoFollowCursor;
			GetComponent<Player>().setTarget(null);
		}
		
		// Rotate player to face target
		if (GetComponent<Player>().getTarget() != null)
		{
			GetComponent<Player>().FaceTarget();
		}
	}
	
	// Function to lock-onto next nearest object from input dictionary
	private bool LockOn(Dictionary<int, GameObject> inObjects)
	{
		if (inObjects == null)
		{
			FindObjectOfType<AudioManager>().CreateAndPlay(this.gameObject, "FailedLockOn");
			return false;
		}
		else
		{
			GameObject currentTarget = GetComponent<Player>().getTarget();
			GameObject nextClosest = null;
			float lockOnDist = GetComponent<IntelligentAgent>().getLockOnRadius();
			float lockOnDistSqrd = lockOnDist * lockOnDist;
			
			if (currentTarget == null)
			{
				// No current target, set as the closest
				nextClosest = GetComponent<IntelligentAgent>().GetClosestObject(inObjects);
				Vector3 nextClosestPos = nextClosest.gameObject.transform.position;
				float closestDistSqrd = (nextClosestPos - this.transform.position).sqrMagnitude;
				if (closestDistSqrd <= lockOnDistSqrd)
				{
					GetComponent<Player>().setTarget(nextClosest);
					return true;
				}
				else
				{
					nextClosest = null;
				}
			}
			else
			{
				// Find next closest after current target
				nextClosest = null;
				Vector3 targetPos = currentTarget.gameObject.transform.position;
				float targetDistSqrd = (targetPos - this.transform.position).sqrMagnitude;
				float minDist = Mathf.Infinity;

				foreach (var objClone in inObjects)
				{
					Vector3 clonePos = objClone.Value.gameObject.transform.position;
					float distSqrd = (clonePos - this.transform.position).sqrMagnitude;
					if (distSqrd < minDist && distSqrd > targetDistSqrd && distSqrd <= lockOnDistSqrd)
					{
						nextClosest = objClone.Value;
						minDist = distSqrd;
					}
				}
			}
			
			// Return boolean on successful target set
			if (nextClosest != null)
			{
				GetComponent<Player>().setTarget(nextClosest);
				return true;
			}
			else
			{
				GetComponent<Player>().setTarget(null);
				return false;
			}
		}
	}
	
	// Function to lock-onto enemy target using key press
	private void enemyLockOn()
	{
		Dictionary<int, GameObject> visibleEnemies = GameManager.getObjectsVisible(GameManager.getEnemies());
		
		if (!LockOn(visibleEnemies))
		{
			// Visible enemies dictionary is null
			Debug.Log("No enemies are visible to screen");
			GetComponent<Player>().setTarget(null);
		}
	}
	
	// Function to lock-onto power up target using key press
	private void powerUpLockOn()
	{
		Dictionary<int, GameObject> visiblePowerUps = GameManager.getObjectsVisible(GameManager.getPowerUps());
		
		if (!LockOn(visiblePowerUps))
		{
			// Visible power up dictionary is null
			Debug.Log("No power ups are visible to screen");
			GetComponent<Player>().setTarget(null);
		}
	}
	
	// Function to lock-onto food target using key press
	private void foodLockOn()
	{
		Dictionary<int, GameObject> visibleFood = GameManager.getObjectsVisible(GameManager.getFood());
		Dictionary<int, GameObject> visibleSuperFood = GameManager.getObjectsVisible(GameManager.getSuperFood());
		Dictionary<int, GameObject> merged;
		
		// Merge dictionaries and make the super food the override value in case of duplicate keys (shoudn't be any)
		if (visibleSuperFood != null && visibleSuperFood.Count > 0)
		{
			merged = new Dictionary<int, GameObject>(visibleFood.Count + visibleSuperFood.Count);
			foreach (var clone in visibleSuperFood)
			{
				merged.Add(clone.Key, clone.Value);
			}
			foreach (var clone in visibleFood)
			{
				if (!visibleSuperFood.ContainsKey(clone.Key))
				{
					merged.Add(clone.Key, clone.Value);
				}
			}
		}
		else
		{
			merged = visibleFood;
		}
		
		if (!LockOn(merged))
		{
			// Visible food/super-food dictionary is null
			Debug.Log("No food or superfood capsules are visible to screen");
			GetComponent<Player>().setTarget(null);
		}
	}
	
	// Function to lock-onto target using a mouse click
	public void mouseLockOn()
	{
		int ignoreMask = ~((1 << 2) | (1 << 8)); // 2: ignore raycast, 8: arena
		RaycastHit hitInfo = new RaycastHit();
		bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, ignoreMask);
		
		if (hit && hitInfo.transform.gameObject.GetComponent<MeshRenderer>().enabled) // successful hit and rendered
		{
			string[] tags = {"Enemy", "Food", "SuperFood", "PowerUp"};
			string targetTag = hitInfo.transform.gameObject.tag;
			GameObject newTarget = null;
			if (tags.Contains(targetTag))
			{
				newTarget = hitInfo.transform.gameObject;
			}
			GetComponent<Player>().setTarget(newTarget);
		}
		else
		{
			Debug.Log("No hit whatsoever...");
			GetComponent<Player>().setTarget(null);
		}
	}
}
