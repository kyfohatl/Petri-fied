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
	
	private Vector3 curDir = new Vector3(0f, 0f, 0f);
	
	// Third-per Camera
	public Transform cam;
	
	// Entity Manager (contains info of relevant entities)
	public GameManager GameManager;

	
	// Start is called before the first frame update
	void Start()
	{
		this.GameManager = FindObjectOfType<GameManager>();
	}
	
	// Update is called once per frame
	void Update()
	{
		// Calculate the target direction of movement
		Vector3 horizontalMoveDir = this.cam.right * Input.GetAxisRaw("Horizontal");
		Vector3 verticalMoveDir = this.cam.forward * Input.GetAxisRaw("Vertical");
		Vector3 hoverMoveDir = this.cam.up * Input.GetAxisRaw("Hover");
		Vector3 targetDir = (horizontalMoveDir + verticalMoveDir + hoverMoveDir).normalized;
		
		if (targetDir.magnitude >= 0.1f)
		{
			// Player is moving
			gameObject.GetComponent<IsMoving>().isMoving = true;
			
			// Check for lock-on target and calculate direction based on acceleration
			GameObject curTarget = this.gameObject.GetComponent<Player>().getTarget();
			if (curTarget == null)
			{
				this.curDir = Vector3.Lerp(curDir, targetDir, acceleration * Time.deltaTime);
			}
			else
			{
				targetDir = (curTarget.transform.position - this.transform.position).normalized;
				this.curDir = Vector3.Lerp(curDir, targetDir, acceleration * Time.deltaTime);
			}
			
			// Rotate the model to face the direction of travel
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), turnSmooth);
			
			// Move towards the current direction
			float geneticSpeedMultiplier = GetComponent<Player>().getSpeedMultiplier() * GetComponent<Player>().getPowerUpSpeedMultiplier();
			float finalSpeedMultiplier = this.speedMultiplier * geneticSpeedMultiplier; // this.speedMultiplier is an artifact from the character controller == 10f
			controller.Move(curDir * finalSpeedMultiplier * Time.deltaTime / transform.localScale.x);
		}
		else
		{
			// Player is not moving
			this.gameObject.GetComponent<IsMoving>().isMoving = false;
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
		if (this.gameObject.GetComponent<Player>().getTarget() != null)
		{
			this.gameObject.GetComponent<Player>().FaceTarget();
		}
	}
	
	// Function to lock-onto enemy target using key press
	private void enemyLockOn()
	{
		Dictionary<int, GameObject> visibleEnemies = GameManager.getEnemiesVisible();
		
		if (visibleEnemies != null)
		{
			GameObject currentTarget = this.gameObject.GetComponent<Player>().getTarget();
			float lockOnDist = this.gameObject.GetComponent<Player>().getLockOnRadius();
			
			if (currentTarget == null)
			{
				// No current target, set as the closest
				GameObject closestObj = this.gameObject.GetComponent<Player>().GetClosestObject(visibleEnemies);
				float closestDist = Vector3.Distance(closestObj.transform.position, this.transform.position);
				if (closestDist <= lockOnDist)
				{
					this.gameObject.GetComponent<Player>().setTarget(closestObj);
				}
			}
			else
			{
				// Find next closest after current target
				GameObject nextClosest = null;
				float targetDist = Vector3.Distance(currentTarget.transform.position, this.transform.position);
				float minDist = Mathf.Infinity;
				
				foreach (var enemyClone in visibleEnemies)
				{
					float dist = Vector3.Distance(enemyClone.Value.transform.position, this.transform.position);
					if (dist < minDist && dist > targetDist && dist <= lockOnDist)
					{
						nextClosest = enemyClone.Value;
						minDist = dist;
					}
				}
				if (nextClosest == null)
				{
					Debug.Log ("No longer locked-on!");
				}
				// Finally set the target as the next closest enemy visible (will be null if visible is exhausted)
				this.gameObject.GetComponent<Player>().setTarget(nextClosest);
			}
		}
		else
		{
			// Visible enemies dictionary is null
			Debug.Log("No enemies are visible to screen");
			this.gameObject.GetComponent<Player>().setTarget(null);
		}
		
		if (this.gameObject.GetComponent<Player>().getTarget() != null)
		{
			GameObject target = this.gameObject.GetComponent<Player>().getTarget();
			string targetName = target.gameObject.GetComponent<Enemy>().getName();
			Debug.Log ("Locked-onto enemy player: " + targetName);
		}
	}
	
	// Function to lock-onto power up target using key press
	private void powerUpLockOn()
	{
		Dictionary<int, GameObject> visiblePowerUps = GameManager.getPowerUpsVisible();
		
		if (visiblePowerUps != null)
		{
			GameObject currentTarget = this.gameObject.GetComponent<Player>().getTarget();
			float lockOnDist = this.gameObject.GetComponent<Player>().getLockOnRadius();
			
			if (currentTarget == null)
			{
				// No current target, set as the closest
				GameObject closestObj = this.gameObject.GetComponent<Player>().GetClosestObject(visiblePowerUps);
				float closestDist = Vector3.Distance(closestObj.transform.position, this.transform.position);
				if (closestDist <= lockOnDist)
				{
					this.gameObject.GetComponent<Player>().setTarget(closestObj);
				}
			}
			else
			{
				// Find next closest after current target
				GameObject nextClosest = null;
				float targetDist = Vector3.Distance(currentTarget.transform.position, this.transform.position);
				float minDist = Mathf.Infinity;
				
				foreach (var powerUpClone in visiblePowerUps)
				{
					float dist = Vector3.Distance(powerUpClone.Value.transform.position, this.transform.position);
					if (dist < minDist && dist > targetDist && dist <= lockOnDist)
					{
						nextClosest = powerUpClone.Value;
						minDist = dist;
					}
				}
				if (nextClosest == null)
				{
					Debug.Log ("No longer locked-on!");
				}
				// Finally set the target as the next closest power up visible (will be null if visible is exhausted)
				this.gameObject.GetComponent<Player>().setTarget(nextClosest);
			}
		}
		else
		{
			// Visible power up dictionary is null
			Debug.Log("No power ups are visible to screen");
			this.gameObject.GetComponent<Player>().setTarget(null);
		}
		
		if (this.gameObject.GetComponent<Player>().getTarget() != null)
		{
			Debug.Log ("Locked-onto Power-Up");
		}
	}
	
	// Function to lock-onto food target using key press
	private void foodLockOn()
	{
		Dictionary<int, GameObject> visibleFood = GameManager.getFoodVisible();
		
		if (visibleFood != null)
		{
			GameObject currentTarget = this.gameObject.GetComponent<Player>().getTarget();
			float lockOnDist = this.gameObject.GetComponent<Player>().getLockOnRadius();
			
			if (currentTarget == null)
			{
				// No current target, set as the closest
				GameObject closestObj = this.gameObject.GetComponent<Player>().GetClosestObject(visibleFood);
				float closestDist = Vector3.Distance(closestObj.transform.position, this.transform.position);
				if (closestDist <= lockOnDist)
				{
					this.gameObject.GetComponent<Player>().setTarget(closestObj);
				}
			}
			else
			{
				// Find next closest after current target
				GameObject nextClosest = null;
				float targetDist = Vector3.Distance(currentTarget.transform.position, this.transform.position);
				float minDist = Mathf.Infinity;
				
				foreach (var foodClone in visibleFood)
				{
					float dist = Vector3.Distance(foodClone.Value.transform.position, this.transform.position);
					if (dist < minDist && dist > targetDist && dist <= lockOnDist)
					{
						nextClosest = foodClone.Value;
						minDist = dist;
					}
				}
				if (nextClosest == null)
				{
					Debug.Log ("No longer locked-on!");
				}
				// Finally set the target as the next closest enemy visible (will be null if visible is exhausted)
				this.gameObject.GetComponent<Player>().setTarget(nextClosest);
			}
		}
		else
		{
			// Visible power up dictionary is null
			Debug.Log("No food capsules are visible to screen");
			this.gameObject.GetComponent<Player>().setTarget(null);
		}
		
		if (this.gameObject.GetComponent<Player>().getTarget() != null)
		{
			Debug.Log ("Locked-onto food");
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
				Debug.Log ("Locked-onto enemy player: " + hitInfo.transform.gameObject.GetComponent<Enemy>().getName());
				this.gameObject.GetComponent<Player>().setTarget(hitInfo.transform.gameObject);
			}
			else if (targetTag == "Food")
			{
				Debug.Log ("Locked-onto food");
				this.gameObject.GetComponent<Player>().setTarget(hitInfo.transform.gameObject);
			}
			else if (targetTag == "PowerUp")
			{
				Debug.Log("Locked-onto Power-Up");
				// Debug.Log ("Locked-onto Power-Up: " + hitInfo.transform.gameObject.GetComponent<PowerUp>().getName());
				this.gameObject.GetComponent<Player>().setTarget(hitInfo.transform.gameObject);
			}
			else if (this.gameObject.GetComponent<Player>().getTarget() != null) // remove lock-on
			{
				Debug.Log ("No longer locked-on!");
				this.gameObject.GetComponent<Player>().setTarget(null);
			}
		}
		else
		{
			Debug.Log("No hit whatsoever...");
		}
	}
}
