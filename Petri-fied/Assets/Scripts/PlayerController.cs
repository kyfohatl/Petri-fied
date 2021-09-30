using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IsMoving))]
public class PlayerController : MonoBehaviour
{
	[SerializeField]
	private CharacterController controller;
	[SerializeField]
	private float speedMultiplier;
	[SerializeField]
	private float acceleration = 2f;
	// Controls how smoothly the player turns from one direction to another
	[SerializeField]
	private float turnSmooth = 0.05f;
	public Transform cam;
	
	private Vector3 curDir = new Vector3(0f, 0f, 0f);
	
	// Start is called before the first frame update
	void Start()
	{
		this.speedMultiplier = 10f;
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
			float geneticSpeedMultiplier = this.gameObject.GetComponent<Player>().getSpeedMultiplier();
			float finalSpeedMultiplier = this.speedMultiplier * geneticSpeedMultiplier;
			controller.Move(curDir * finalSpeedMultiplier * Time.deltaTime / transform.localScale.x);
		}
		else
		{
			// Player is not moving
			this.gameObject.GetComponent<IsMoving>().isMoving = false;
		}
		
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hitInfo = new RaycastHit();
			bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
			
			if (hit)
			{
				string targetTag = hitInfo.transform.gameObject.tag;
				if (targetTag == "Enemy")
				{
					Debug.Log ("Locked-onto: " + hitInfo.transform.gameObject.GetComponent<Enemy>().getName());
					this.gameObject.GetComponent<Player>().setTarget(hitInfo.transform.gameObject);
				}
				else if (targetTag == "Food")
				{
					Debug.Log ("Locked-onto: " + hitInfo.transform.gameObject.tag);
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
				Debug.Log("No hit");
			}
		}
	}
}
