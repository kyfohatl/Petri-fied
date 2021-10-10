using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaSize : MonoBehaviour
{
    public float ArenaRadius = 50.0f;
    public float ArenaHeight = 20.0f;
    private float basicScale = 1.0f;
    private float HeightRefactor = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3(ArenaRadius/basicScale, ArenaRadius/basicScale, ArenaHeight/basicScale*HeightRefactor );
        
    }
}
