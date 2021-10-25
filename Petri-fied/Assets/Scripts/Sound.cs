using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    [Range(0f, 1f)]
    public float spatialBlend = 1f;

    [Range(0.1f, 10f)]
    public float minDistance = 1f;
    [Range(0f, 500f)]
    public float maxDistance = 200f;


    public bool loop;

    [HideInInspector]
    public AudioSource source;

}
