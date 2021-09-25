using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IsMoving))]
public class PlayerController : MonoBehaviour
{
  [SerializeField]
  private CharacterController controller;
  [SerializeField]
  private float speedMultiplier = 10f;
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
  }

  // Update is called once per frame
  void Update()
  {
    // Calculate the target direction of movement
    Vector3 horizontalMoveDir = cam.right * Input.GetAxisRaw("Horizontal");
    Vector3 verticalMoveDir = cam.forward * Input.GetAxisRaw("Vertical");
    Vector3 hoverMoveDir = cam.up * Input.GetAxisRaw("Hover");
    Vector3 tgtDir = (horizontalMoveDir + verticalMoveDir + hoverMoveDir).normalized;

    if (tgtDir.magnitude >= 0.1f)
    {
      // Player is moving
      gameObject.GetComponent<IsMoving>().isMoving = true;

      // Calculate direction based on acceleration
      curDir = Vector3.Lerp(curDir, tgtDir, acceleration * Time.deltaTime);

      // Rotate the model to face the direction of travel
      transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(tgtDir), turnSmooth);
      
      // Move towards the current direction
      controller.Move(curDir * speedMultiplier * Time.deltaTime / transform.localScale.x);
    }
    else
    {
      // Player is not moving
      gameObject.GetComponent<IsMoving>().isMoving = false;
    }
  }
}
