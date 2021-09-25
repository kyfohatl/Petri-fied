using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    // Enemy prefab
    public GameObject Enemy;
    
    // Spawn rate of food
    public float spawnRate; // per second
    private float deltaSpawn; // delay between spawn generations
    
    public int spawnMin = 1; // lower bound spawns per generation
    public int spawnMax = 1; // upper bound spawns per generation
    public int spawnLimit = 5; // maximum spawns allowed
    
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
            
            // Loop to spawn one or more food objects per generation
            for (int i = 0; i < spawnCount; i++)
            {
                Generate();
            }
        }
    }
    
    // Function to instantiate new food objects into the scene: needs updating to map to petri-dish dimensions (can be a child of the petri dish to get dimensions from parent is my idea)
    void Generate()
    {
        // Below variables currently spawn enemies
        float x = Random.Range(-50, 50);
        float y = Random.Range(-10, 10);
        float z = Random.Range(-50, 50);

        Vector3 Target = new Vector3(x, y, z);
        
        // Actually generates the food and sets as child of spawner
        GameObject spawned = Instantiate(Enemy, Target, Quaternion.identity, transform);
    }
}
