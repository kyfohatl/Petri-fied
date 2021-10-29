using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
  public float RotationSpeed = 15f;

  private void Update()
  {
    if (!GameManager.inst.gameOver || !transform)
    {
      return;
    }
    transform.Rotate(Vector3.up * Time.deltaTime * RotationSpeed);
  }
}
