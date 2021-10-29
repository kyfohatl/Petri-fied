using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
  public static GameEvents instance;

  // Call on start-up of game
  private void Awake()
  {
    instance = this;
	DontDestroyOnLoad(this.gameObject);
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
