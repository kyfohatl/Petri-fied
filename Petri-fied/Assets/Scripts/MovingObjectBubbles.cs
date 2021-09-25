using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IsMoving))]
public class MovingObjectBubbles : MonoBehaviour
{
  // The GameObject containing the bubble particle system
  public GameObject bubbleEffectObject;
  private ParticleSystem bubbleEffect;
  // The previous motion status of the GameObject
  private bool wasMoving;
  // The emission rate whilst moving
  [SerializeField]
  private float movingEmissionRate = 5f;
  // The emission rate whist stationary
  [SerializeField]
  private float stationaryEmissionRate = 0.5f;

  // Start is called before the first frame update
  void Start()
  {
    wasMoving = false;
    bubbleEffect = bubbleEffectObject.GetComponent<ParticleSystem>();

    // Set the initial emission rate to be stationary
    var emission = bubbleEffect.emission;
    emission.rateOverTime = stationaryEmissionRate;
  }

  // Update is called once per frame
  void Update()
  {
    // If the motion status has changed, changed the emission rate
    bool isMoving = gameObject.GetComponent<IsMoving>().isMoving;
    if (isMoving != wasMoving)
    {
      // Motion status has changed
      wasMoving = isMoving;
      var emission = bubbleEffect.emission;
      if (isMoving)
      {
        // Faster emission when moving
        emission.rateOverTime = movingEmissionRate;
      }
      else
      {
        // Slower emission when stationary
        emission.rateOverTime = stationaryEmissionRate;
      }
    }
  }
}
