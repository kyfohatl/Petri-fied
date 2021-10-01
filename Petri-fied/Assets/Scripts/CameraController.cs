using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  // The main camera
  public Camera cam;
  // The player transform
  public Transform playerPos;

  // The camera orbit distance from the player character
  [SerializeField]
  private float orbitDistance = 5f;
  // Influences the sensitivity of camera movement
  [SerializeField]
  private float cameraSensitivityMultiplier = 1f;

  // The current horizontal rotation of the camera, in degrees, around the player
  private float curHorizontalRotation = 0f;
  // The current vertical rotation of the camera, in degrees, around the player
  private float curVerticalRotation = 0f;

  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
    // Get movement input
    float hMovementInput = Input.GetAxisRaw("Mouse X");
    float vMovementInput = Input.GetAxis("Mouse Y");

    // Increment horizontal and vertical rotation angles using input
    curHorizontalRotation = incrementAngle(curHorizontalRotation, hMovementInput);
    curVerticalRotation = incrementAngle(curVerticalRotation, vMovementInput);

    Vector3 newPosOffset = new Vector3(
      orbitDistance * Mathf.Cos(curHorizontalRotation) * Mathf.Sin(curVerticalRotation),
      orbitDistance * Mathf.Cos(curVerticalRotation),
      orbitDistance * Mathf.Sin(curHorizontalRotation) * Mathf.Sin(curVerticalRotation)
    );

    cam.transform.position = playerPos.position + newPosOffset;
  }

  // Increments the given angle by the given amount, taking into account framerate, and returns the result
  private float incrementAngle(float angle, float amount)
  {
    return (angle + amount * cameraSensitivityMultiplier * Time.deltaTime) % (2 * Mathf.PI);
  }

  private void LateUpdate()
  {
    // Always make sure the camera is looking at the player
    cam.transform.LookAt(playerPos);
  }
}
