using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Leaderboard : MonoBehaviour
{
  public static Leaderboard Instance { get; private set; }

  private void Awake()
  {
    Instance = this;
  }

  // List of agents in the game
  private List<IntelligentAgent> leaderboardAgents;

  // UI elements
  // public TMP_Text leaderboardDisplay;

  // Gameobjects
  public GameObject RowTemplate;

  // List of agents in the game
  private List<GameObject> renderedRows;

  // Start is called before the first frame update
  void Start()
  {
    leaderboardAgents = new List<IntelligentAgent>();
    renderedRows = new List<GameObject>();
    // LoadLeaderboard();

    AddAgent(GameManager.get().Player.GetComponent<IntelligentAgent>());
  }

  // Update is called once per frame
  void Update()
  {
  }


  public void RenderLeaderboard()
  {
    // Clear old UI
    foreach (GameObject obj in renderedRows)
    {
      Destroy(obj);
    }

    // Get top score
    int topScore = 0;
    IntelligentAgent topScoreAgent = null;
    IntelligentAgent playerAgent = null;
    bool playerIsInTopShown = false;
    foreach (IntelligentAgent agent in leaderboardAgents)
    {
      if (agent.getScore() > topScore)
      {
        topScore = agent.getScore();
        topScoreAgent = agent;
      }
      Debug.Log(agent);
      Debug.Log(agent.GetType().ToString().Equals("Player"));
      if (agent.GetType().ToString().Equals("Player"))
      {
        playerAgent = agent;
      }
    }

    // Render UI
    int i = 1;
    foreach (IntelligentAgent agent in leaderboardAgents)
    {
      // Show max 3 spots
      if (i > 3 && !playerIsInTopShown)
      {
        break;
      }
      else if (i > 4)
      {
        break;
      }

      if (agent.GetType().ToString().Equals("Player"))
      {
        playerIsInTopShown = true;
      }

      GameObject row = Instantiate(RowTemplate) as GameObject;
      row.SetActive(true);

      if (agent.Equals(topScoreAgent))
      {
        row.GetComponent<LeaderboardUIRow>().SetIsTopScore(true);
      }
      if (agent.Equals(playerAgent))
      {
        row.GetComponent<LeaderboardUIRow>().SetIsPlayer(true);
      }
      row.GetComponent<LeaderboardUIRow>().SetRank(i.ToString());
      row.GetComponent<LeaderboardUIRow>().SetName(agent.getName());
      row.GetComponent<LeaderboardUIRow>().SetScore(agent.getScore().ToString());

      row.transform.SetParent(RowTemplate.transform.parent, false);
      renderedRows.Add(row);
      i++;
    }
    if (i > 3 && !playerIsInTopShown)
    {
      foreach (IntelligentAgent agent in leaderboardAgents)
      {
        if (agent.Equals(playerAgent))
        {
          GameObject row = Instantiate(RowTemplate) as GameObject;
          row.SetActive(true);
          row.GetComponent<LeaderboardUIRow>().SetIsPlayer(true);

          row.GetComponent<LeaderboardUIRow>().SetRank(i.ToString());
          row.GetComponent<LeaderboardUIRow>().SetName(agent.getName());
          row.GetComponent<LeaderboardUIRow>().SetScore(agent.getScore().ToString());

          row.transform.SetParent(RowTemplate.transform.parent, false);
          renderedRows.Add(row);
          i++;
        }
      }
    }
  }

  public void AddAgent(IntelligentAgent agent)
  {
    leaderboardAgents.Add(agent);
    SortLeaderboard();
  }
  public void RemoveAgent(IntelligentAgent agent)
  {
    leaderboardAgents.Remove(agent);
    SortLeaderboard();
  }

  public void SortLeaderboard()
  {
    for (int i = leaderboardAgents.Count - 1; i > 0; i--)
    {
      if (leaderboardAgents[i].Score > leaderboardAgents[i - 1].Score)
      {
        // Temp
        IntelligentAgent temp = leaderboardAgents[i - 1];

        leaderboardAgents[i - 1] = leaderboardAgents[i];
        leaderboardAgents[i] = temp;
      }
    }
    UpdatePlayerPrefsString();
  }

  void UpdatePlayerPrefsString()
  {
    string stats = "";

    for (int i = 0; i < leaderboardAgents.Count; i++)
    {
      stats += leaderboardAgents[i].getName() + ",";
      stats += leaderboardAgents[i].getScore() + ",";

    }

    PlayerPrefs.SetString("Leaderboards", stats);
    UpdateLeaderboardUI();
  }

  void UpdateLeaderboardUI()
  {
    // leaderboardDisplay.text = "";
    // for (int i = 0; i <= leaderboardAgents.Count - 1; i++)
    // {
    //   leaderboardDisplay.text += leaderboardAgents[i].getName() + " : " + leaderboardAgents[i].getScore() + "\n";
    // }
    RenderLeaderboard();
  }

  void LoadLeaderboard()
  {
    ClearPrefs();
    Dictionary<int, GameObject> allAgentsDict = GameManager.get().getEnemies();
    allAgentsDict.Add(GameManager.get().Player.GetInstanceID(), GameManager.get().Player);
    foreach (KeyValuePair<int, GameObject> t in allAgentsDict)
    {
      IntelligentAgent agent = t.Value.GetComponent<IntelligentAgent>();
      leaderboardAgents.Add(agent);
    }

    allAgentsDict.Clear();
    SortLeaderboard();

    // Code to load from storage
    // string stats = PlayerPrefs.GetString("Leaderboards", "");

    // string[] stats2 = stats.Split(",");

    // for (int i = 0; i < stats2.Length- 2; i +=2){
    //   IntelligentAgent p = new IntelligentAgent()
    // }
  }

  void ClearPrefs()
  {
    PlayerPrefs.DeleteAll();
    leaderboardAgents.Clear();
    // leaderboardDisplay.text = "";
  }

  public List<IntelligentAgent> GetLeaderboardAgents()
  {
    return this.leaderboardAgents;
  }
}
