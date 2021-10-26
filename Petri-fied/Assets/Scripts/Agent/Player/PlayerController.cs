using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IsMoving))]
public class PlayerController : MonoBehaviour
{
	// Basic parameters
	[SerializeField] private CharacterController controller;
	[SerializeField] private float speedMultiplier = 10f;
	[SerializeField] private float acceleration = 2f;
	[SerializeField] private float turnSmooth = 0.05f;
	public bool AlwaysFollowTarget;
	
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
		Vector3 targetDir = (horizontalMoveDir + verticalMoveDir + hoverMoveDir).normalized;
		GameObject curTarget = GetComponent<Player>().getTarget();
		
		if (targetDir.magnitude >= 0.1f || (this.AlwaysFollowTarget && curTarget != null))
		{
			// Player is moving
			GetComponent<IsMoving>().isMoving = true;
			
			// Check for lock-on target and calculate direction based on acceleration
			if (curTarget == null)
			{
				this.curDir = Vector3.Slerp(curDir, targetDir, acceleration * Time.deltaTime);
				// Rotate the model to face the direction of travel
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), turnSmooth);
			}
			else
			{
				targetDir = (curTarget.transform.position - this.transform.position).normalized;
				this.curDir = targetDir;
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
			
			if (currentTarget == null)
			{
				// No current target, set as the closest
				nextClosest = GetComponent<IntelligentAgent>().GetClosestObject(inObjects);
				float closestDist = Vector3.Distance(nextClosest.gameObject.transform.position, this.transform.position);
				if (closestDist <= lockOnDist)
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
				float targetDist = Vector3.Distance(currentTarget.gameObject.transform.position, this.transform.position);
				float minDist = Mathf.Infinity;
				
				foreach (var objClone in inObjects)
				{
					float dist = Vector3.Distance(objClone.Value.gameObject.transform.position, this.transform.position);
					if (dist < minDist && dist > targetDist && dist <= lockOnDist)
					{
						nextClosest = objClone.Value;
						minDist = dist;
					}
				}
			}
			// Return boolean on successful target set
			if (nextClosest == null)
			{
				Debug.Log("No locked-onto anything!");
				FindObjectOfType<AudioManager>().CreateAndPlay(this.gameObject, "FailedLockOn");
				return false;
			}
			else
			{
				// Finally set the target as the next closest object visible
				GetComponent<Player>().setTarget(nextClosest);
				return true;
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
		
		if (GetComponent<Player>().getTarget() != null)
		{
			GameObject target = GetComponent<Player>().getTarget();
			string targetName = target.gameObject.GetComponent<Enemy>().getName();
			Debug.Log("Locked-onto enemy player: " + targetName);
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
		
		if (GetComponent<Player>().getTarget() != null)
		{
			Debug.Log("Locked-onto Power-Up");
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
			Debug.Log("No food capsules are visible to screen");
			GetComponent<Player>().setTarget(null);
		}
		
		if (GetComponent<Player>().getTarget() != null)
		{
			string targetTag = GetComponent<Player>().getTarget().tag;
			Debug.Log("Locked-onto " + targetTag);
		}
	}
	
	// Function to lock-onto target using a mouse click
	public void mouseLockOn()
	{
		RaycastHit hitInfo = new RaycastHit();
		bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
		
		if (hit) // ray cast intersects something not null
		{
			string targetTag = hitInfo.transform.gameObject.tag;
			if (targetTag == "Enemy")
			{
				Debug.Log("Locked-onto enemy player: " + hitInfo.transform.gameObject.GetComponent<Enemy>().getName());
				GetComponent<Player>().setTarget(hitInfo.transform.gameObject);
			}
			else if (targetTag == "Food" || targetTag == "SuperFood")
			{
				Debug.Log("Locked-onto " + targetTag);
				GetComponent<Player>().setTarget(hitInfo.transform.gameObject);
			}
			else if (targetTag == "PowerUp")
			{
				Debug.Log("Locked-onto Power-Up");
				// Debug.Log("Locked-onto Power-Up: " + hitInfo.transform.gameObject.GetComponent<PowerUp>().getName());
				GetComponent<Player>().setTarget(hitInfo.transform.gameObject);
			}
			else if (GetComponent<Player>().getTarget() != null) // remove lock-on
			{
				Debug.Log("No longer locked-on!");
				GetComponent<Player>().setTarget(null);
			}
		}
		else
		{
			Debug.Log("No hit whatsoever...");
			FindObjectOfType<AudioManager>().CreateAndPlay(this.gameObject, "FailedLockOn");
			GetComponent<Player>().setTarget(null);
		}
	}
}
