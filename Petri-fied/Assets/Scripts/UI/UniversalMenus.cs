using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UniversalMenus : MonoBehaviour
{

  public static bool GameIsPaused = false;

  public GameObject pauseMenuUi;
  public GameObject settingsUi;

  public TMP_Text CameraSensitivityValue;

  private void Awake()
  {
    DontDestroyOnLoad(this.gameObject);
  }

  public void Start()
  { }

  public void QuitGame()
  {
    Debug.Log("Quit Game");
    Application.Quit();
  }

  public void Resume()
  {
    pauseMenuUi.SetActive(false);
    settingsUi.SetActive(false);
    Time.timeScale = 1f;
    GameIsPaused = false;
  }

  public void Pause()
  {
    pauseMenuUi.SetActive(true);
    Time.timeScale = 0f;
    GameIsPaused = true;
  }
  public void Restart()
  {
    GameOverMenu.RestartGame();
  }

  public void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      if (GameManager.inst.gameOver)
      {
        return;
      }
      if (GameIsPaused)
      {
        Resume();
      }
      else
      {
        Pause();
      }
    }

    // NOTE: If you update min max sensitivity in unity then you must update here.
    float minSens = 0.05f;
    float maxSens = 5f;
    CameraController cam = FindObjectOfType<CameraController>();
    float sensitivityPercentage = ((cam.cameraSensitivityMultiplier - minSens) / (maxSens - minSens)) * 100;
    if (this.CameraSensitivityValue)
    {
      this.CameraSensitivityValue.text = Math.Round(sensitivityPercentage).ToString() + "%";
    }
  }
}

