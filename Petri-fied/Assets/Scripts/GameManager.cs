using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  // Instantiate this object
  public static GameManager inst;
  public bool gameOver = false;

  // Variety of game entities
  public GameObject Player;
  private Dictionary<int, GameObject> Food;
  private Dictionary<int, GameObject> Enemies;
  private Dictionary<int, GameObject> PowerUps;
  

  // Call on start-up of game
  void Awake()
  {
    inst = this;
    Player = GameObject.FindGameObjectWithTag("Player");
    Food = new Dictionary<int, GameObject>();
    Enemies = new Dictionary<int, GameObject>();
    PowerUps = new Dictionary<int, GameObject>();
  }

  // Function for adding food to manager dictionary
  public static void AddFood(int id, GameObject obj)
  {
    if (inst.Food.ContainsKey(id))
    {
      Debug.Log("Duplicate registration attempted... " + id.ToString());
      return;
    }

    inst.Food.Add(id, obj);
    // Debug.Log(inst.Food.Count);
  }

  // Function for adding an enemy to manager dictionary
  public static void AddEnemy(int id, GameObject obj)
  {
    if (inst.Enemies.ContainsKey(id))
    {
      Debug.Log("Duplicate registration attempted... " + id.ToString());
      return;
    }

    inst.Enemies.Add(id, obj);
    // Debug.Log(inst.Enemies.Count);
  }

  // Function for adding a PowerUp to manager dictionary
  public static void AddPowerUp(int id, GameObject obj)
  {
    if (inst.PowerUps.ContainsKey(id))
    {
      Debug.Log("Duplicate registration attempted... " + id.ToString());
      return;
    }

    inst.PowerUps.Add(id, obj);
    // Debug.Log(inst.PowerUps.Count);
  }

  // Function for removing food from manager dictionary
  public static void RemoveFood(int id)
  {
    if (!inst.Food.ContainsKey(id))
    {
      Debug.Log("No object with this ID... " + id.ToString());
      return;
    }

    inst.Food.Remove(id);
    // Debug.Log(inst.Food.Count);
  }

  // Function for removing an enemy from manager dictionary
  public static void RemoveEnemy(int id)
  {
    if (!inst.Enemies.ContainsKey(id))
    {
      Debug.Log("No object with this ID... " + id.ToString());
      return;
    }

    inst.Enemies.Remove(id);
    Leaderboard.Instance.RemoveAgent(inst.Enemies[id].GetComponent<IntelligentAgent>());
    // Debug.Log(inst.Enemies.Count);
  }

  // Function for removing a Power Up from manager dictionary
  public static void RemovePowerUp(int id)
  {
    if (!inst.PowerUps.ContainsKey(id))
    {
      Debug.Log("No object with this ID... " + id.ToString());
      return;
    }

    inst.PowerUps.Remove(id);
    // Debug.Log(inst.PowerUps.Count);
  }

  // Function to determine interactable game objects visible to screen
  public Dictionary<int, GameObject> getEnemiesVisible()
  {
    Dictionary<int, GameObject> visibleObjects = new Dictionary<int, GameObject>();

    foreach (KeyValuePair<int, GameObject> enemyClone in Enemies)
    {
      Vector3 screenPoint = Camera.main.WorldToViewportPoint(enemyClone.Value.transform.position);
      bool visibleToScreen = screenPoint.z > 0
                   && screenPoint.x > 0
                   && screenPoint.x < 1
                   && screenPoint.y > 0
                   && screenPoint.y < 1;
      if (visibleToScreen)
      {
        visibleObjects.Add(enemyClone.Key, enemyClone.Value);
      }
    }

    if (visibleObjects.Count == 0)
    {
      // No enemies are visible to screen
      return null;
    }

    return visibleObjects;
  }

  // Get all food in the world
  public Dictionary<int, GameObject> getFood()
  {
    return this.Food;
  }

  // Get all enemies in the world
  public Dictionary<int, GameObject> getEnemies()
  {
    return this.Enemies;
  }
 
  // Get all PowerUps in the world
  public Dictionary<int, GameObject> getPowerUps()
  {
    return this.PowerUps;
  }


  // Get instance of this GameManager
  public static GameManager get()
  {
    return inst;
  }
}
