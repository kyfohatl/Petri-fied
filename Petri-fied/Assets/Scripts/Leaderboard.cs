using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Leaderboard : MonoBehaviour
{

  // The number of positions to show in the leaderboard.
  public int PositionsVisible = 4;

  public static Leaderboard Instance { get; private set; }

  // List of agents in the game
  private List<IntelligentAgent> leaderboardAgents;

  // Gameobjects
  public GameObject RowTemplate;

  // List of agents in the game
  private List<GameObject> renderedRows;

  private void Awake()
  {
    Instance = this;
  }

  void Start()
  {
    leaderboardAgents = new List<IntelligentAgent>();
    renderedRows = new List<GameObject>();

    // Make sure the player is in the leaderboard.
    AddAgent(GameManager.get().Player.GetComponent<IntelligentAgent>());
  }

  void Update()
  {
  }

  /**
  Function to (re)render the leaderboard UI.
  */
  public void UpdateLeaderboardUI()
  {
    // Clear old UI.
    foreach (GameObject obj in renderedRows)
    {
      Destroy(obj);
    }

    // Get the top scoring agent + player agent (and whether it is in top positions).
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
      // Show correct number of max top spots depending on whether player is in them or not.
      if (i > PositionsVisible - 1 && !playerIsInTopShown)
      {
        break;
      }
      else if (i > PositionsVisible)
      {
        break;
      }

      // Check if it's a player row.
      if (agent.GetType().ToString().Equals("Player"))
      {
        playerIsInTopShown = true;
      }

      // Render the LeaderboardUIRow.
      GameObject row = Instantiate(RowTemplate) as GameObject;
      row.SetActive(true);

      // Apply top score styling if #1.
      if (agent.Equals(topScoreAgent))
      {
        row.GetComponent<LeaderboardUIRow>().SetIsTopScore(true);
      }

      // Apply player styling if === player.
      if (agent.Equals(playerAgent))
      {
        row.GetComponent<LeaderboardUIRow>().SetIsPlayer(true);
      }

      // Set UIRow data.
      row.GetComponent<LeaderboardUIRow>().SetRank(i.ToString());
      row.GetComponent<LeaderboardUIRow>().SetName(agent.getName());
      row.GetComponent<LeaderboardUIRow>().SetScore(agent.getScore().ToString());

      // Assign UI row to the parent (Leaderboard UI) GameObject.
      row.transform.SetParent(RowTemplate.transform.parent, false);

      // Store this row as one we rendered so we can delete it for next re-render.
      renderedRows.Add(row);
      i++;
    }

    // Conditional to show player agent at the bottom if they weren't in the top x positions.
    if (i > PositionsVisible && !playerIsInTopShown)
    {
      foreach (IntelligentAgent agent in leaderboardAgents)
      {
        if (agent.Equals(playerAgent))
        {
          // Render the LeaderboardUIRow.
          GameObject row = Instantiate(RowTemplate) as GameObject;
          row.SetActive(true);

          // Add player styling to row.
          row.GetComponent<LeaderboardUIRow>().SetIsPlayer(true);

          // Set UIRow data.
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

  /**
  Function to add the agent to the leaderboard and re-sort/rerender the board.
  */
  public void AddAgent(IntelligentAgent agent)
  {
    leaderboardAgents.Add(agent);
    SortLeaderboard();
  }

  /**
  Function to remove the agent from the leaderboard and re-sort/rerender the board.
  */
  public void RemoveAgent(IntelligentAgent agent)
  {
    leaderboardAgents.Remove(agent);
    SortLeaderboard();
  }

  /**
  Function to reorder and rerender the leaderboard UI.
  */
  public void SortLeaderboard()
  {
    // Map through local leaderboard state.
    for (int i = leaderboardAgents.Count - 1; i > 0; i--)
    {
      if (leaderboardAgents[i].Score > leaderboardAgents[i - 1].Score)
      {
        // Temp store agent with lesser score.
        IntelligentAgent temp = leaderboardAgents[i - 1];

        // Swap agents.
        leaderboardAgents[i - 1] = leaderboardAgents[i];
        leaderboardAgents[i] = temp;
      }
    }

    // Rerender the UI.
    UpdateLeaderboardUI();
  }

  /**
  Function to initiate, load and sort/rerender the leaderboard data from the GameManager.
  */
  void LoadLeaderboard()
  {
    // Reset the stored prefs state.
    ClearLeaderboard();

    // Add enemies and player together and then populate the leaderboard agents.
    Dictionary<int, GameObject> allAgentsDict = GameManager.get().getEnemies();
    allAgentsDict.Add(GameManager.get().Player.GetInstanceID(), GameManager.get().Player);
    foreach (KeyValuePair<int, GameObject> t in allAgentsDict)
    {
      IntelligentAgent agent = t.Value.GetComponent<IntelligentAgent>();
      leaderboardAgents.Add(agent);
    }

    // Clear the temp dictionary.
    allAgentsDict.Clear();

    // Resort the leaderboard.
    SortLeaderboard();
  }

  /**
  Function to clear the leaderboard local data.
  */
  void ClearLeaderboard()
  {
    leaderboardAgents.Clear();
  }

  /**
  Function to get all agents in the leaderboard (players and enemies in the game).
  */
  public List<IntelligentAgent> GetLeaderboardAgents()
  {
    return this.leaderboardAgents;
  }
}
