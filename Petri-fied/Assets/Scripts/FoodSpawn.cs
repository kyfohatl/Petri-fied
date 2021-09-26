using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawn : MonoBehaviour
{
    // Food prefab
    public GameObject Food;
    
    // Spawn rate of food
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
        // Below variables currently spawn food
        float x = Random.Range(transform.position.x - 50, transform.position.x + 50);
        float y = Random.Range(transform.position.y - 10, transform.position.y + 10);
        float z = Random.Range(transform.position.z - 50, transform.position.z + 50);
        
        Vector3 Target = new Vector3(x, y, z);
        
        // Actually generates the food and sets as child of spawner
        GameObject spawned = Instantiate(Food, Target, Random.rotation, transform);
        
        // Track the food spawned.
        GameManager.AddFood(spawned.GetInstanceID(), spawned);
    }
}
