using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardUIRow : MonoBehaviour
{

  public TMP_Text nameText;

  public TMP_Text scoreText;

  public TMP_Text rankText;

  public TMP_FontAsset MediumFont;


  private bool isPlayer;

  private bool isTopScore;

  private void ApplyStyles()
  {
    if (isPlayer)
    {
      rankText.color = new Color32(6, 234, 138, 255);
    }
    if (isTopScore || isPlayer)
    {
      nameText.font = MediumFont;
      scoreText.font = MediumFont;
      rankText.font = MediumFont;
    }
  }
  public void SetName(string name)
  {
    nameText.text = name;
    ApplyStyles();
  }
  public void SetScore(string score)
  {
    scoreText.text = score;
    ApplyStyles();
  }

  public void SetRank(string rank)
  {
    rankText.text = "#" + rank;
    ApplyStyles();
  }
  public void SetIsPlayer(bool tf)
  {
    this.isPlayer = tf;
    ApplyStyles();
  }
  public void SetIsTopScore(bool tf)
  {
    this.isTopScore = tf;
    ApplyStyles();
  }
}
