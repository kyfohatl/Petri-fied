using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
  // Instantiate this object
  public static GameManager inst;
  public bool gameOver = false;

  // Variety of game entities
  public GameObject Player;
  private Dictionary<int, GameObject> Food;
  private Dictionary<int, GameObject> SuperFood;
  private Dictionary<int, GameObject> Enemies;
  private Dictionary<int, GameObject> PowerUps;

  // Main player game hud
  public GameObject PlayerBasicUI;

  // Difficulty sliders (all multipliers)
  public float enemySpeedBoost = 1f;
  public float enemyGrowthBoost = 1f;
  public float enemyAggressionMultiplier = 1f;


  // Call on start-up of game
  void Awake()
  {
    inst = this;
    Player = GameObject.FindGameObjectWithTag("Player");
    Food = new Dictionary<int, GameObject>();
    SuperFood = new Dictionary<int, GameObject>();
    Enemies = new Dictionary<int, GameObject>();
    PowerUps = new Dictionary<int, GameObject>();

    DontDestroyOnLoad(this.gameObject);
  }

  public void EndGameForPlayer()
  {
    // Don't animate fade out on death.
    SceneManager.LoadScene(2);

    GameManager.inst.SetGameOver(true);
    FindObjectOfType<Player>().setPosition(new Vector3(-249.52f, 486, FindObjectOfType<Player>().Radius / 1.5f));
    Camera.main.transform.rotation = Quaternion.Euler(2, 8, 0);
    Camera.main.transform.position = new Vector3(-18.2f, 8.1f, -100);
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

  // Function for adding super food to manager dictionary
  public static void AddSuperFood(int id, GameObject obj)
  {
    if (inst.SuperFood.ContainsKey(id))
    {
      Debug.Log("Duplicate registration attempted... " + id.ToString());
      return;
    }
    inst.SuperFood.Add(id, obj);
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
    // Add agent to the dictionary and leaderboard
    inst.Enemies.Add(id, obj);
    Leaderboard.Instance.AddAgent(obj.GetComponent<IntelligentAgent>());
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
    // Debug.Log("Food was removed from game manager; remaining count: " + inst.Food.Count);
  }

  // Function for removing super food from manager dictionary
  public static void RemoveSuperFood(int id)
  {
    if (!inst.SuperFood.ContainsKey(id))
    {
      Debug.Log("No object with this ID... " + id.ToString());
      return;
    }
    inst.SuperFood.Remove(id);
    // Debug.Log("SuperFood was removed from game manager; remaining count: " + inst.SuperFood.Count);
  }

  // Function for removing an enemy from manager dictionary
  public static void RemoveEnemy(int id)
  {
    if (!inst.Enemies.ContainsKey(id))
    {
      Debug.Log("No object with this ID... " + id.ToString());
      return;
    }

    // Remove agent from the leaderboard and dictionary
    Leaderboard.Instance.RemoveAgent(inst.Enemies[id].GetComponent<IntelligentAgent>());
    inst.Enemies.Remove(id);
    // Debug.Log("Enemy was removed from game manager; remaining count: " + inst.Enemies.Count);
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
    // Debug.Log("PowerUp was removed from game manager; remaining count: " + inst.PowerUps.Count);
  }

  // Function to determine game objects visible to screen from provided dictionary
  public Dictionary<int, GameObject> getObjectsVisible(Dictionary<int, GameObject> objs)
  {
    if (objs.Count == 0)
    {
      // Provided dictionary is empty
      return null;
    }

    Dictionary<int, GameObject> visibleObjs = new Dictionary<int, GameObject>();

    foreach (KeyValuePair<int, GameObject> objClone in objs)
    {
      Vector3 screenPoint = Camera.main.WorldToViewportPoint(objClone.Value.transform.position);
      bool visibleToScreen = screenPoint.z > 0
                 && screenPoint.x > 0
                 && screenPoint.x < 1
                 && screenPoint.y > 0
                 && screenPoint.y < 1;
      if (visibleToScreen)
      {
        if (!Physics.Linecast(objClone.Value.transform.position, Camera.main.transform.position))
        {
          // Nothing obstructs the visible enemy with the main camera
          visibleObjs.Add(objClone.Key, objClone.Value);
        }
      }
    }

    if (visibleObjs.Count == 0)
    {
      // No objects from the specified dictionary are visible to screen
      return null;
    }
    return visibleObjs;
  }

  // Get all food in the world
  public Dictionary<int, GameObject> getFood()
  {
    return this.Food;
  }

  // Get all super food in the world
  public Dictionary<int, GameObject> getSuperFood()
  {
    return this.SuperFood;
  }

  // Get all enemies in the world
  public Dictionary<int, GameObject> getEnemies()
  {
    return this.Enemies;
  }

  // Get all power ups in the world
  public Dictionary<int, GameObject> getPowerUps()
  {
    return this.PowerUps;
  }

  // Get instance of this GameManager
  public static GameManager get()
  {
    return inst;
  }

  // Set whether the game is over.
  public void SetGameOver(bool tf)
  {
    this.gameOver = tf;
    PlayerBasicUI.SetActive(!tf);
  }

}
