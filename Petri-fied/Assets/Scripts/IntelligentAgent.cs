using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelligentAgent : MonoBehaviour
{
  // Game manager to load other entities
  public GameManager GameManager;

  // Target: what the agent is currently Locked-onto
  public GameObject Target;

  // Agent data elements
  public string Name = "JoeMama-420";
  public int Score = 1;
  public float Radius = 1f;
  private float LockOnRadius;
  
  //Power up trackers
  private float PowerUpSpeedMultiplier = 1f;
  private bool InvincibilityMode = false;

  // Agent genetic modifiers
  [SerializeField] private float FoodGrowthMultiplier = 1f;
  [SerializeField] private float ScoreDecayMultiplier = 1f;
  [SerializeField] private float SpeedMultiplier = 1f;
  [SerializeField] private float LockOnRadiusMultiplier = 5f;

  // Agent statistics
  private int peakScore = 1;
  private float initialisationTime;
  private float decayTimer = 0f; // used for tracking time
  private float decayDelta; // derived: time between unit score decays
  private float decayExcess = 0f; // excess accrued from rounding error

  // Function to intialise variables after instantiation, called in start
  public void StartLife()
  {
    this.initialisationTime = Time.timeSinceLevelLoad;
    this.GameManager = FindObjectOfType<GameManager>();
    GenerateRandomGenetics();
    this.LockOnRadius = this.Radius * this.LockOnRadiusMultiplier;
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
      if (!other.gameObject.GetComponent<Enemy>().isInvincible() || !this.InvincibilityMode)
      {
        Enemy otherPlayer = other.gameObject.GetComponent<Enemy>();
        int scoreDifference = this.Score - otherPlayer.getScore();

        if (scoreDifference > 0)
        {
          UpdateScore(otherPlayer.getScore());
          AssimilateGenetics(otherPlayer);
          Debug.Log(this.Name + "has eaten: " + otherPlayer.getName());
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
	else if (other.gameObject.tag == "PowerUp")
	{
		
	}
  }
  
  // Function to check if agent is invincible
  public bool isInvincible()
  {
	  return this.InvincibilityMode;
  }

  // Function to update radius
  public virtual void UpdateRadius()
  {
    this.Radius = (float)Mathf.Pow(this.Score, 1f / 3f);
    this.LockOnRadius = this.LockOnRadiusMultiplier * this.Radius;
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
    Leaderboard.Instance.SortLeaderboard();
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
      this.decayExcess += Time.deltaTime / this.decayDelta - 1;
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

  // Function to generate random starting genetics
  public void GenerateRandomGenetics()
  {
    float foodGrowthMin = 1.0f;
    this.FoodGrowthMultiplier = Mathf.Max(foodGrowthMin, Mathf.Abs(normalRandom(1f, 1f))); // mean: 1, std: 1

    float speedMultMin = 1.0f;
    this.SpeedMultiplier = Mathf.Max(speedMultMin, Mathf.Abs(normalRandom(1f, 1f))); // mean: 1, std: 1

    float scoreDecayMax = 5f;
    this.ScoreDecayMultiplier = Mathf.Min(scoreDecayMax, Mathf.Abs(normalRandom(1f, 0.2f))); // mean: 1, std: 0.2

    float lockOnRadiusMin = 25f;
    this.LockOnRadiusMultiplier = Mathf.Max(lockOnRadiusMin, Mathf.Abs(normalRandom(50f, 10f))); // mean: 50, std: 10
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

  // Function to generate normally distributed random number
  public float normalRandom(float mean, float stdDev)
  {
    float x = Random.Range(0f, 1f);
    float y = Random.Range(0f, 1f);
    float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(x)) * Mathf.Sin(2f * Mathf.PI * y);
    float output = mean + stdDev * randStdNormal;

    return output;
  }

  // Rotate agent towards at target
  public void FaceTarget()
  {
    Vector3 direction = (Target.transform.position - transform.position).normalized;
    Quaternion lookRotation = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 1f);
  }

  // Find nearest object in a dictionary
  public GameObject GetClosestObject(Dictionary<int, GameObject> objs)
  {
	  GameObject closest = null;
	  float minDist = Mathf.Infinity;
	  Vector3 currentPos = this.transform.position;
	  float epsilon = 1e-4f;
	  
	  foreach (KeyValuePair<int, GameObject> t in objs)
	  {
		  Vector3 directionToObject = t.Value.transform.position - currentPos;
		  float distSqrToTarget = directionToObject.sqrMagnitude;
		  
		  if (distSqrToTarget < minDist && distSqrToTarget > epsilon)
		  {
			  closest = t.Value;
			  minDist = distSqrToTarget;
		  }
	  }
	  
	  return closest;
  }

  // Setter method for Power Up speed multiplier
  public void setPowerUpSpeedMultiplier(float NewMult)
  {
    this.PowerUpSpeedMultiplier = NewMult;
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

  // Getter method for lock-on radius
  public float getLockOnRadius()
  {
    return this.LockOnRadius;
  }

  // Getter method for Power Up speed multiplier
  public float getPowerUpSpeedMultiplier()
  {
    return this.PowerUpSpeedMultiplier;
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
}
