using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{

  public TMP_Text GameOverStats;

  // Specify the different leaderboard from the gameover scene.
  public Leaderboard Leaderboard;

  void Start()
  {
    Leaderboard.LoadLeaderboard();
    Invoke(nameof(DelayedGameOverCleanup), 0.85f);
  }

  private void DelayedGameOverCleanup()
  {
    Leaderboard.Instance.ShouldUpdate = false;
    int hours = TimeSpan.FromSeconds(Player.instance.getSurvivalTime()).Hours;
    int minutes = TimeSpan.FromSeconds(Player.instance.getSurvivalTime()).Minutes;
    int seconds = TimeSpan.FromSeconds(Player.instance.getSurvivalTime()).Seconds;

    string timeAliveFormatted = hours.ToString() + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");
    int score = Player.instance.getScore();
    int rank = Leaderboard.Instance.playerRank;
    GameOverStats.text = $"You survived for {timeAliveFormatted} and finished at position #{rank} with a score of {score}.";
  }

  public void RestartGame()
  {
    Leaderboard.Instance.ClearLeaderboard();
    Destroy(GameObject.Find("GameManager"));
    Destroy(GameObject.Find("Food Spawner"));
    Destroy(GameObject.Find("Enemy Spawner"));
    Destroy(GameObject.Find("PowerUp Spawner"));
    Destroy(GameObject.Find("Leaderboard"));
    Destroy(GameObject.Find("Player"));
	Destroy(GameObject.Find("Arena"));
	Destroy(GameObject.Find("AudioManager"));
    Destroy(GameObject.Find("UICanvas"));
    GameObject.Find("Main Camera").name = "Old Camera";
    LevelLoader.Instance.LoadNextLevel(0);
  }
}
