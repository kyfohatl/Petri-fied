using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawn : MonoBehaviour
{
  // prefab
  public GameObject PowerUp;

  // Spawn rate of PowerUps
  public float spawnRate; // per second
  private float deltaSpawn; // delay between spawn generations

  public int spawnMin = 1; // lower bound spawns per generation
  public int spawnMax = 1; // upper bound spawns per generation
  public int spawnLimit = 100; // maximum spawns allowed

  private float timer; // clock to track time between generations

  // Start is called before the first frame update
  void Start()
  {
    deltaSpawn = 1f / spawnRate;
    timer = deltaSpawn;
  }

  // Update is called once per frame
  void Update()
  {
    timer -= Time.deltaTime;

    if (timer < 0 && transform.childCount < spawnLimit)
    {
      timer = deltaSpawn;
      int spawnCount = Random.Range(spawnMin, spawnMax);

      // Loop to spawn one or more powerUp objects per generation
      for (int i = 0; i < spawnCount; i++)
      {
        Generate();
      }
    }
  }

  // Function to instantiate new PowerUp objects into the world
  void Generate()
  {
    // Determine spawn position
    float radius = 50f;
    float height = 20f;

    Vector2 coord = Random.insideUnitCircle * radius;
    float y = Random.Range(transform.position.z - height / 2f, transform.position.z + height / 2f);

    Vector3 Target = new Vector3(transform.position.x + coord.x, transform.position.y + y, transform.position.z + coord.y);

    // Actually generates the PowerUp and sets as child of spawner
    GameObject spawned = Instantiate(PowerUp, Target, Random.rotation, transform);

    // Track the PowerUp spawned.
    GameManager.AddPowerUp(spawned.GetInstanceID(), spawned);
  }
}