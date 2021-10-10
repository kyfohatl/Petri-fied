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
			float geneticSpeedMultiplier = this.gameObject.GetComponent<Player>().getSpeedMultiplier() * this.gameObject.GetComponent<Player>().getPowerUpSpeedMultiplier();
			float finalSpeedMultiplier = this.speedMultiplier * geneticSpeedMultiplier;
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
						Debug.Log("Distance: " + closestDist);
						Debug.Log("Locked-onto enemy player: " + closestObj.gameObject.GetComponent<Enemy>().getName());
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
					
					// Finally set the target as the next closest enemy visible in the scene
					this.gameObject.GetComponent<Player>().setTarget(nextClosest);
					if (nextClosest != null)
					{
						Debug.Log("Distance: " + minDist);
						Debug.Log("Locked-onto enemy player: " + nextClosest.gameObject.GetComponent<Enemy>().getName());
					}
					else
					{
						Debug.Log ("No longer locked-on!");
					}
				}
			}
			else
			{
				Debug.Log("No enemies are visible to screen");
			}
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
			else if (targetTag == "Power-Up")
			{
				// Debug.Log ("Locked-onto Power-Up: " + hitInfo.transform.gameObject.GetComponent<PowerUp>().getName());
				this.gameObject.GetComponent<Player>().setTarget(hitInfo.transform.gameObject);
			}
			else // remove lock-on
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
