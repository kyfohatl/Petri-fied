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
  private float orbitDistanceMult = 10f;
  // Influences the sensitivity of camera movement
  [SerializeField]
  private float cameraSensitivityMultiplier = 1f;

  // The current horizontal rotation of the camera, in degrees, around the player
  private float curHorizontalRotation = 0f;
  // The current vertical rotation of the camera, in degrees, around the player
  private float curVerticalRotation = 0f;
  // The current orbit distance of the camera
  private float curOrbitDistance;
  // The orbit distance the camera is aiming to get to
  private float goalOrbitDistance;

  // Start is called before the first frame update
  void Start()
  {
    // Set ant event on GameEvents to change camera zoom upon player scale change
    GameEvents.instance.onPlayerRadiusChange += onPlayerRadiusChange;

    curOrbitDistance = orbitDistanceMult;
    goalOrbitDistance = curOrbitDistance;
  }

  // Update is called once per frame
  void Update()
  {
    // Get movement input
    float hMouseInput = Input.GetAxisRaw("Mouse X");
    float vMouseInput = Input.GetAxisRaw("Mouse Y");
    float hKeyboardInput = Input.GetAxisRaw("Horizontal Look");
    float vKeyboardInput = Input.GetAxisRaw("Vertical Look");

    float hMovementInput = getMaxInputVal(hMouseInput, hKeyboardInput);
    float vMovementInput = getMaxInputVal(vMouseInput, vKeyboardInput);

    // Increment horizontal and vertical rotation angles using input
    curHorizontalRotation = incrementAngle(curHorizontalRotation, hMovementInput);
    curVerticalRotation = incrementAngle(curVerticalRotation, vMovementInput);

    Vector3 newPosOffset = new Vector3(
      curOrbitDistance * Mathf.Cos(curHorizontalRotation) * Mathf.Sin(curVerticalRotation),
      curOrbitDistance * Mathf.Cos(curVerticalRotation),
      curOrbitDistance * Mathf.Sin(curHorizontalRotation) * Mathf.Sin(curVerticalRotation)
    );

    cam.transform.position = playerPos.position + newPosOffset;

    // Camera scaling
    // If the camera is not at the goal orbit distance, interpolate orbit distance till it gets there
    if (Mathf.Abs(goalOrbitDistance - curOrbitDistance) > 0.1f)
    {
      curOrbitDistance = Mathf.Lerp(curOrbitDistance, goalOrbitDistance, 0.01f);
    }
  }

  private void LateUpdate()
  {
    // Always make sure the camera is looking at the player
    cam.transform.LookAt(playerPos);
  }

  // Increments the given angle by the given amount, taking into account framerate, and returns the result
  private float incrementAngle(float angle, float amount)
  {
    return (angle + amount * cameraSensitivityMultiplier * Time.deltaTime) % (2 * Mathf.PI);
  }

  // Changes the camera distance to the player to match the new given player radius
  private void onPlayerRadiusChange(float radius)
  {
    goalOrbitDistance = orbitDistanceMult + 3f * radius;
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
}
