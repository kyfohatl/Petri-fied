using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
  [SerializeField]
  private float forwardSpeedMult = 10f, strafeSpeedMult = 5f, hoverSpeedMult = 5f;
  private float forwardVelocity, strafeVelocity, hoverVelocity;
  private float forwardAcceleration = 2.5f, strafeAcceleration = 2f, hoverAcceleration = 2f;

  [SerializeField]
  private float rotationSpeed = 90f;
  private Vector2 mousePos, screenCentre, mouseToCentreDist;

  // Start is called before the first frame update
  void Start()
  {
    screenCentre.x = Screen.width / 2.0f;
    screenCentre.y = Screen.height / 2.0f;
  }

  // Update is called once per frame
  void Update()
  {
    mouseToCentreDist.x = (Input.mousePosition.x - screenCentre.x) / screenCentre.x;
    // TODO: Check if division by screenCentre.y is correct below
    mouseToCentreDist.y = (Input.mousePosition.y - screenCentre.y) / screenCentre.y;

    mouseToCentreDist = Vector2.ClampMagnitude(mouseToCentreDist, 1f);

    transform.Rotate(
      -mouseToCentreDist.y * rotationSpeed * Time.deltaTime,
      mouseToCentreDist.x * rotationSpeed * Time.deltaTime,
      0f,
      Space.Self
    );

    forwardVelocity = Mathf.Lerp(forwardVelocity, forwardSpeedMult * Input.GetAxisRaw("Vertical"), forwardAcceleration * Time.deltaTime);
    strafeVelocity = Mathf.Lerp(strafeVelocity, strafeSpeedMult * Input.GetAxisRaw("Horizontal"), strafeAcceleration * Time.deltaTime);
    hoverVelocity = Mathf.Lerp(hoverVelocity, hoverSpeedMult * Input.GetAxisRaw("Hover"), hoverAcceleration * Time.deltaTime);

    transform.position += transform.forward * forwardVelocity * Time.deltaTime;
    transform.position += transform.right * strafeVelocity * Time.deltaTime;
    transform.position += transform.up * hoverVelocity * Time.deltaTime;
  }
}
