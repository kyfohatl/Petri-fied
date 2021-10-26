using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaSize : MonoBehaviour
{
	// Instantiate this object
	public static ArenaSize instance;
	
	// Dimensions
    public float ArenaRadius = 150.0f;
    public float ArenaHeight = 100.0f;
    public float basicScale = 1.0f;
    private float HeightRefactor = 0.5f;
	
	// Call on start-up of game
	private void Awake()
	{
		instance = this;
		DontDestroyOnLoad(this.gameObject);
	}
	
    // Start is called before the first frame update
    void Start()
    {
		float diameter = 2f * this.ArenaRadius;
        transform.localScale = new Vector3(diameter, this.ArenaHeight * this.HeightRefactor, diameter) * this.basicScale;
    }

    // Update is called once per frame
    void Update()
    {
		
    }
}
