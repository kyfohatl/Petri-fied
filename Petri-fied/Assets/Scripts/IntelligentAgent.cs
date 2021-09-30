using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelligentAgent : MonoBehaviour
{
<<<<<<< HEAD
  // Agent data elements
  public string Name = "JoeMama-420";
  public int Score = 1;
  public float Radius = 1f;

  // Agent genetic modifiers
  public float FoodGrowthMultiplier;
  public float SpeedMultiplier;
  public float ScoreDecayRate; // percentage of score loss per decay

  // Agent statistics
  private int peakScore;
  private float initialisationTime;

  // Game manager to load other entities
  public GameManager GameManager;

  // Default Constructor
  public IntelligentAgent(string givenName)
  {
    initialisationTime = Time.timeSinceLevelLoad;
    Name = givenName;
    GenerateRandomGenetics();
  }

  // Function to generate random starting genetics
  public void GenerateRandomGenetics()
  {
    this.FoodGrowthMultiplier = UnityEngine.Random.Range(1f, 3f);
    this.SpeedMultiplier = UnityEngine.Random.Range(1f, 3f);
    this.ScoreDecayRate = UnityEngine.Random.Range(0.01f, 0.05f);
  }

  // Function called on collisions
  void OnTriggerEnter(Collider other)
  {
    if (other.gameObject.tag == "Food")
    {
      int increase = (int)Mathf.Round(this.FoodGrowthMultiplier);
      UpdateScore(increase);
      Destroy(other.gameObject);
      GameManager.RemoveFood(other.gameObject.GetInstanceID());
    }
    else if (other.gameObject.tag == "Enemy")
    {
      if (other.gameObject.GetComponent("Enemy") != null)
      {
        Enemy otherPlayer = other.gameObject.GetComponent<Enemy>();
        int scoreDifference = this.Score - otherPlayer.getScore();

        if (scoreDifference > 0)
        {
          UpdateScore(otherPlayer.getScore());
          Destroy(other.gameObject);
          GameManager.RemoveEnemy(other.gameObject.GetInstanceID());
        }
        else if (scoreDifference < 0)
        {
          Destroy(gameObject);
          GameManager.RemoveEnemy(gameObject.GetInstanceID());
        }
      }
    }
  }

  // Function to update radius
  public virtual void UpdateRadius()
  {
    this.Radius = (float)Mathf.Pow(this.Score, 1f / 3f);
  }

  // Function to update Score: called by collision events
  public void UpdateScore(int amount)
  {
    this.Score += amount;
    UpdateRadius();
  }

  // Function to update size
  public void UpdateSize()
  {
    float sizeChangeSpeed = 5.0f;
    float sizeDifference = this.Radius - transform.localScale.x;

    float X = transform.localScale.x + sizeDifference;
    float Y = transform.localScale.y + sizeDifference;
    float Z = transform.localScale.z + sizeDifference;
    Vector3 newScale = new Vector3(X, Y, Z);

    transform.localScale = Vector3.Lerp(transform.localScale, newScale, sizeChangeSpeed * Time.deltaTime);
  }

  // Function to implement score decay
  public void DecayScore()
  {
    int reduction = (int)Mathf.Floor(this.ScoreDecayRate * this.Score);
    if (this.Score > 1)
    {
      // Minimum reduction per decay is 1, take min between calculated neg reduction and -1
      reduction = (int)Mathf.Min(-reduction, -1);
      UpdateScore(reduction);
    }
  }

  // Getter method for name
  public string getName()
  {
    return this.Name;
  }

  // Getter method for score
  public int getScore()
  {
    return this.Score;
  }

  // Getter method for radius
  public float getRadius()
  {
    return this.Radius;
  }

  // Getter method for speed multiplier
  public float getSpeedMultiplier()
  {
    return this.SpeedMultiplier;
  }

  // Getter method for initialisation time
  public float getInitialisationTime()
  {
    return this.initialisationTime;
  }

  // Getter method for peak score
  public float getPeakScore()
  {
    return this.peakScore;
  }
=======
	// Agent data elements
	public string Name = "JoeMama-420";
	public int Score = 1;
	public float Radius = 1f;
	
	// Agent genetic modifiers
	[SerializeField] private float FoodGrowthMultiplier;
	[SerializeField] private float ScoreDecayMultiplier;
	[SerializeField] private float SpeedMultiplier;
	
	// Agent statistics
	private int peakScore;
	private float initialisationTime;
	private float decayTimer = 0f; // used for tracking time
	private float decayDelta; // derived: time between unit score decays
	private float decayExcess = 0f; // excess accrued from rounding error
	
	// Game manager to load other entities
	public GameManager GameManager;
	
	// Target: what the agent is currently Locked-onto
	public GameObject Target;
	
	// Function to intialise variables after instantiation, called in start
	public void StartLife()
	{
		this.initialisationTime = Time.timeSinceLevelLoad;
		GenerateRandomGenetics();
	}
	
	// Function to generate random starting genetics
	public void GenerateRandomGenetics()
	{
		this.FoodGrowthMultiplier = UnityEngine.Random.Range(1f, 3f);
		this.SpeedMultiplier = UnityEngine.Random.Range(1f, 3f);
		this.ScoreDecayMultiplier = UnityEngine.Random.Range(0.01f, 2f);
	}
	
	// Function called on collisions
	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Food")
		{
			int increase = (int)Mathf.Round(this.FoodGrowthMultiplier);
			UpdateScore(increase);
			Destroy(other.gameObject);
			GameManager.RemoveFood(other.gameObject.GetInstanceID());
		}
		else if (other.gameObject.tag == "Enemy")
		{
			if (other.gameObject.GetComponent("Enemy") != null)
			{
				Enemy otherPlayer = other.gameObject.GetComponent<Enemy>();
				int scoreDifference = this.Score - otherPlayer.getScore();
				
				if (scoreDifference > 0)
				{
					UpdateScore(otherPlayer.getScore());
					AssimilateGenetics(otherPlayer);
					Destroy(other.gameObject);
					GameManager.RemoveEnemy(other.gameObject.GetInstanceID());
				}
				else if (scoreDifference < 0)
				{
					otherPlayer.AssimilateGenetics(this);
					Destroy(gameObject);
					GameManager.RemoveEnemy(gameObject.GetInstanceID());
				}
			}
		}
	}
	
	// Function to update radius
	public virtual void UpdateRadius()
	{
		this.Radius = (float)Mathf.Pow(this.Score, 1f / 3f);
	}
	
	// Function to update Score: called by collision events
	public void UpdateScore(int amount)
	{
		this.Score += amount;
		if (this.Score > this.peakScore)
		{
			this.peakScore = this.Score;
		}
		UpdateRadius();
	}
	
	// Function to update size
	public void UpdateSize()
	{
		float sizeChangeThreshold = 0.05f; // if size difference is greater than 0.05 scale units
		float sizeChangeSpeed = 5.0f;
		float sizeDifference = this.Radius - transform.localScale.x;
		
		// Prevents constant need to lerp scale if the size change is small
		if (Mathf.Abs(sizeDifference) > sizeChangeThreshold)
		{
			float X = transform.localScale.x + sizeDifference;
			float Y = transform.localScale.y + sizeDifference;
			float Z = transform.localScale.z + sizeDifference;
			Vector3 newScale = new Vector3(X, Y, Z);
			
			transform.localScale = Vector3.Lerp(transform.localScale, newScale, sizeChangeSpeed * Time.deltaTime);
		}
	}
	
	// Function to implement score decay
	public void DecayScore()
	{
		this.decayTimer += Time.deltaTime; // increase timer
		int ratePerSecondAt100 = 5;
		this.decayDelta = ratePerSecondAt100 * 100f / (this.ScoreDecayMultiplier * this.Score); // time between decays
		int reductionAmount = -1;
		
		if (this.decayDelta <= Time.deltaTime) // only called when score decay outpaces fps
		{
			// track decay loss using an excess tracker and add every integer of decay to reduction
			this.decayExcess +=  Time.deltaTime / this.decayDelta - 1;
			if (this.decayExcess >= 1)
			{
				reductionAmount -= (int)Mathf.Floor(this.decayExcess);
			}
		}
		
		if (this.Score > 1 && this.decayTimer >= this.decayDelta)
		{
			// Reduce score by one scaled unit and reset timer
			UpdateScore(reductionAmount);
			this.decayTimer = 0f;
		}
	}
	
	// Function to take on superior genetics of eaten agent
	public void AssimilateGenetics(IntelligentAgent prey)
	{
		if (prey.getFoodGrowthMultiplier() > this.FoodGrowthMultiplier) // higher = better
		{
			this.FoodGrowthMultiplier = prey.getFoodGrowthMultiplier();
		}
		if (prey.getScoreDecayMultiplier() < this.ScoreDecayMultiplier) // lower = worse
		{
			this.ScoreDecayMultiplier = prey.getScoreDecayMultiplier();
		}
		if (prey.getSpeedMultiplier() > this.SpeedMultiplier) // higher = better, unless you suck at flying like I do
		{
			this.SpeedMultiplier = prey.getSpeedMultiplier();
		}
	}
	
	// Function to set the target of the agent
	public void setTarget(GameObject obj)
	{
		this.Target = obj;
	}
	
	//Function to get the target of the agent
	public GameObject getTarget()
	{
		return this.Target;
	}
	
	// Getter method for name
	public string getName()
	{
		return this.Name;
	}
	
	// Getter method for score
	public int getScore()
	{
		return this.Score;
	}
	
	// Getter method for radius
	public float getRadius()
	{
		return this.Radius;
	}
	
	// Getter method for speed multiplier
	public float getSpeedMultiplier()
	{
		return this.SpeedMultiplier;
	}
	
	// Getter method for food growth multiplier
	public float getFoodGrowthMultiplier()
	{
		return this.FoodGrowthMultiplier;
	}
	
	// Getter method for score decay multiplier
	public float getScoreDecayMultiplier()
	{
		return this.ScoreDecayMultiplier;
	}
	
	// Getter method for initialisation time
	public float getInitialisationTime()
	{
		return this.initialisationTime;
	}
	
	// Getter method for peak score
	public float getPeakScore()
	{
		return this.peakScore;
	}
>>>>>>> shader_2-implementation
}
