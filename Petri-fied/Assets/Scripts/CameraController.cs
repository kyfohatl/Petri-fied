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
  // The current position of the camera along its orbit
  private Vector3 curOrbitOffset;

  // Start is called before the first frame update
  void Start()
  {
    curOrbitOffset = playerPos.position + new Vector3(0f, 0f, orbitDistance);
  }

  // Update is called once per frame
  void Update()
  {
    float hMovementInput = Input.GetAxisRaw("HorizontalLook");
    float vMovementInput = Input.GetAxis("VerticalLook");
    Vector3 hOffset = new Vector3(0f, 0f, 0f);
    Vector3 vOffset = new Vector3(0f, 0f, 0f);

    bool orbitCam = false;

    // new horizontal position
    if (Mathf.Abs(hMovementInput) > 0.1f)
    {
      orbitCam = true;

      // Find out new offset distance from the player position
      curHorizontalRotation = incrementAngle(curHorizontalRotation, hMovementInput);
      hOffset.x = orbitDistance * Mathf.Cos(curHorizontalRotation * Mathf.Deg2Rad);
      hOffset.z = orbitDistance * Mathf.Sin(curHorizontalRotation * Mathf.Deg2Rad);
    }

    // new vertical position
    if (Mathf.Abs(vMovementInput) > 0.1f)
    {
      orbitCam = true;

      // Find out new offset distance from the player position
      curVerticalRotation = incrementAngle(curVerticalRotation, vMovementInput);
      vOffset.x = orbitDistance * Mathf.Cos(curVerticalRotation * Mathf.Deg2Rad);
      vOffset.y = orbitDistance * Mathf.Sin(curVerticalRotation * Mathf.Deg2Rad);
    }

    // Always make sure the camera is looking at the player
    cam.transform.LookAt(playerPos);

    if (orbitCam)
    {
      // Calculate the camera's orbit offset position
      curOrbitOffset = hOffset + vOffset;
    }

    // Finally set the camera position with respect to the player position
    cam.transform.position = playerPos.position + curOrbitOffset;
  }

  // Increments the given angle by the given amount, taking into account framerate, and returns the result
  private float incrementAngle(float angle, float amount)
  {
    return (angle + amount * cameraSensitivityMultiplier * Time.deltaTime) % 360f;
  }
}
