using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    //Sound Manager

    //to play a sound run following line
    // Where Test1 is name of sound in AudioManager
    //FindObjectOfType<AudioManager>().CreateAndPlay(this.gameObject,"Test1");

    public Sound[] sounds;
    
    // Start is called before the first frame update
    void Start()
    {
        //Add core sounds here e.g. background music
        CreateAndPlay(this.gameObject,"BackgroundMusic");
        
    }

    public void CreateAndPlay(GameObject obj, string name){
        //Get sound from list
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null){
            Debug.Log("Sound: " + name + " was not found");
            return;
        }

        //create/get audio component
        if(obj.GetComponent<AudioSource>() == null){
            s.source = obj.AddComponent<AudioSource>();
        }else{
             s.source = obj.GetComponent<AudioSource>();
        }

        //Copy sound info into object Component
        s.source.clip = s.clip;
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.spatialBlend = s.spatialBlend;
        s.source.minDistance = s.minDistance;
        s.source.maxDistance = s.maxDistance;
        s.source.loop = s.loop;

        //Play Sound
        s.source.Play();
    }
}
