using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  public static GameManager inst;

  Dictionary<int, GameObject> Food;
  Dictionary<int, GameObject> Enemies;

  void Awake()
  {
    inst = this;
    Food = new Dictionary<int, GameObject>();
    Enemies = new Dictionary<int, GameObject>();
  }

  // Functions for adding / removing food and emenies from tracker

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
    return Food;
  }

  // Get all enemies in the world
  public Dictionary<int, GameObject> getEnemies()
  {
    return Enemies;
  }

  // Get instance of this gamemanger
  public static GameManager get()
  {
    return inst;
  }
}
