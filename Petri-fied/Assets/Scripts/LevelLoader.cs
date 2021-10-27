using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class LevelLoader : MonoBehaviour
{
  public static LevelLoader Instance;
  public Animator Transition;
  public float TransitionTime = 1.0f;

  public void Awake()
  {
    Instance = this;
  }

  public void LoadNextLevel(int index, Action onResult)
  {
    StartCoroutine(LoadLevel(index, onResult));
  }
  public void LoadNextLevel(int index)
  {
    StartCoroutine(LoadLevel(index, () => { }));
  }

  IEnumerator LoadLevel(int index, Action onResult)
  {
    // Play animation.
    Transition.SetTrigger("Start");

    // Wait to stop playing for x seconds.
    yield return new WaitForSeconds(TransitionTime);

    // Load scene.
    SceneManager.LoadScene(index);

    // Run callback (things to do after scene is loaded).
    onResult();
	ActiveScene(index);
  }
  
  IEnumerator ActiveScene(int index)
  {
	  yield return new WaitForSeconds(0f); // allow frame to finish
	  SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(index));
  }
}
