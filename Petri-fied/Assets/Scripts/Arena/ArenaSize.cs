using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaSize : MonoBehaviour
{
	// Instantiate this object
	public static ArenaSize instance;
	
	// Dimensions
    public float ArenaRadius = 150f; // spawn radius
	
	// Call on start-up of game
	private void Awake()
	{
		instance = this;
		DontDestroyOnLoad(this.gameObject);
	}
	
    // Start is called before the first frame update
    void Start()
    {
		
    }

    // Update is called once per frame
    void Update()
    {
		
    }
}
