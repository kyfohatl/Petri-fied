using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{

  private TMP_InputField _inputField;

  CameraController Camera;

  public void Start()
  {
    Camera = GameObject.Find("Main Camera").GetComponent<CameraController>();
    GameObject oldCam = GameObject.Find("Old Camera");
    if (oldCam != null)
    {
      Destroy(oldCam);
    }
    PlayerPrefs.SetString("Name", null);
    _inputField = GameObject.Find("NameInput").GetComponent<TMP_InputField>();
    _inputField.Select();
    _inputField.ActivateInputField();
  }

  void Update()
  {
    GameObject oldCam = GameObject.Find("Old Camera");
    if (oldCam != null)
    {
      Destroy(oldCam);
    }
  }

  public void PlayGame()
  {
    if (PlayerPrefs.GetString("Name", "") == "")
    {
      FindObjectOfType<AudioManager>().CreateAndPlay(this.gameObject, "InvalidName");
      return;
    }
    FindObjectOfType<AudioManager>().CreateAndPlay(this.gameObject, "StartGame");
    LevelLoader.Instance.LoadNextLevel(1, () =>
    {
      GameManager.get().SetGameOver(false);
      Player.instance.setPosition(new Vector3(0, 0, 0));
      Player.instance.setName(PlayerPrefs.GetString("Name"));
      Camera.InitialiseCameraPosition();
    });
  }

  public void QuitGame()
  {
    Debug.Log("Quit Game");
    Application.Quit();
  }

  public void InputName()
  {
    PlayerPrefs.SetString("Name", _inputField.text);
  }
}
