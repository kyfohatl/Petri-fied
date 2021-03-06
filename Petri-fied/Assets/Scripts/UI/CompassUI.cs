using UnityEngine;
using UnityEngine.UI;

public class CompassUI : MonoBehaviour
{
  public RawImage compass;
  public Transform target;

  void Start()
  {
    // Initialise the (image) rect to show the correct direction.
	compass.uvRect = new Rect(target.transform.localEulerAngles.y / 360f, 0, 1, 1);
  }

  void Update()
  {
    // Update the (image) rect to show the correct direction.
    compass.uvRect = new Rect(target.transform.localEulerAngles.y / 360f, 0, 1, 1);
  }
}
