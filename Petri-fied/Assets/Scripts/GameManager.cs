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

  // Call on start-up of game
  void Awake()
  {
    inst = this;
    Player = GameObject.FindGameObjectWithTag("Player");
    Food = new Dictionary<int, GameObject>();
    Enemies = new Dictionary<int, GameObject>();
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
    Debug.Log(inst.Food.Count);
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
    Debug.Log(inst.Enemies.Count);
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
    Debug.Log(inst.Food.Count);
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
    Debug.Log(inst.Enemies.Count);
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

  // Get instance of this GameManager
  public static GameManager get()
  {
    return inst;
  }
}
