using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
  public static GameEvents instance;

  private void Awake()
  {
    instance = this;
  }

  // Event fired when the player's radius changes
  public event Action<float> onPlayerRadiusChange;
  public void PlayerRadiusChange(float radius)
  {
    if (onPlayerRadiusChange != null)
    {
      onPlayerRadiusChange(radius);
    }
  }
}