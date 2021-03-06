using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelligentAgent : MonoBehaviour
{
  // Game manager to load other entities
  protected GameManager GameManager;

  // Target: what the agent is currently Locked-onto
  public GameObject Target;

  // Agent data elements
  [SerializeField] private string Name = "";
  public int Score = 1;
  public float Radius = 1f; // the scale factor of the agent
  public float LockOnRadius = 10f;
  public int peakScore = 1;
  private float initialisationTime;

  // Player statistics tracking
  protected float survivalTime = 0.0f;
  protected int killCount = 0;
  protected int foodEatenCount = 0;
  protected int superFoodEatenCount = 0;
  protected int powerUpsCollected = 0;

  // Power up trackers
  private float PowerUpSpeedMultiplier = 1f;
  private float InvinGrowthMultiplier = 1f;
  private int activePowers = 0;
  private int activeSpeed = 0;
  private int activeMagnet = 0;
  private int activeInvin = 0;
  [SerializeField] private bool InvincibilityMode = false;
  private bool SpeedMode = false;
  private bool MagnetMode = false;

  // Agent genetic modifiers
  [SerializeField] private float GeneticGrowthMultiplier = 0.5f;
  [SerializeField] public float FoodGrowthMultiplier = 1f;
  [SerializeField] private float ScoreDecayMultiplier = 1f;
  [SerializeField] public float SpeedMultiplier = 1f;
  [SerializeField] public float LockOnRadiusMultiplier = 15f;

  // Size update parameters
  private float sizeUpdateDuration = 1f; // default: 1 second
  private bool sizeUpdateTriggered = true;
  private float sizeUpdateStartTime;
  private Vector3 previousLocalScale;
  private Vector3 goalLocalScale;

  // Score decay trackers
  private float decayTimer = 0f; // used for tracking time
  private float decayDelta; // derived: time between unit score decays
  private float decayExcess = 0f; // excess accrued from rounding error

  // Function to intialise variables after instantiation, called in start
  public void StartLife()
  {
    this.initialisationTime = Time.timeSinceLevelLoad;
    this.sizeUpdateStartTime = Time.timeSinceLevelLoad;
	UpdateRadius();
	this.transform.localScale = new Vector3(1f, 1f, 1f) * this.Radius;
	this.goalLocalScale = this.transform.localScale;
	this.previousLocalScale = this.transform.localScale;
    this.GameManager = FindObjectOfType<GameManager>();
    GenerateRandomGenetics();
    this.LockOnRadius = this.Radius * this.LockOnRadiusMultiplier;
  }

  // Function called on collisions
  void OnTriggerEnter(Collider other)
  {
    // Check tag of collided object
    if (other.gameObject.tag == "Food")
    {
      int increase = (int)Mathf.Round(this.FoodGrowthMultiplier * InvinGrowthMultiplier);
      UpdateScore(increase);
      GameManager.RemoveFood(other.gameObject.GetInstanceID());
      FindObjectOfType<AudioManager>().CreateAndPlay(this.gameObject, "FoodEaten");
	  
	  // Check to see if the game object has a parent and if it's the food pellets object
	  if (other.transform.parent != null && other.transform.parent.name == "FoodPellets")
	  {
		  other.gameObject.SetActive(false);
		  GameObject spawner = GameObject.FindWithTag("Spawner");
		  spawner.GetComponent<ProceduralSpawner>().foodCount -= 1;
	  }
	  else
	  {
		  // Must be a food object not instantiated by the procedural spawner (delete it)
		  Destroy(other.gameObject);
	  }
      this.foodEatenCount += 1;
    }
    else if (other.gameObject.tag == "SuperFood")
    {
      float increase = Mathf.Max((float)this.Score / 6f, 10f * getFoodGrowthMultiplier()); // 16.67% or a nice chunk of
      UpdateScore(Mathf.FloorToInt(increase));
      GameManager.RemoveFood(other.gameObject.GetInstanceID());
      GameManager.RemoveSuperFood(other.gameObject.GetInstanceID());
      FindObjectOfType<AudioManager>().CreateAndPlay(this.gameObject, "SuperFoodEaten");
	  
	  // Check to see if the game object has a parent and if it's the supers object
	  if (other.transform.parent != null && other.transform.parent.name == "Supers")
	  {
		  GameObject spawner = GameObject.FindWithTag("Spawner");
		  spawner.GetComponent<ProceduralSpawner>().superFoodCount -= 1;
	  }
      Destroy(other.gameObject); // this one can always be deleted regardless who instantiated it
      this.superFoodEatenCount += 1;
    }
    else if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Player")
    {
      IntelligentAgent otherAgent = other.gameObject.GetComponent<IntelligentAgent>();
      int scoreDifference = this.Score - otherAgent.getScore();

      if (scoreDifference > 0 && !otherAgent.isInvincible())
      {
        UpdateScore(otherAgent.getScore());
        AssimilateGenetics(otherAgent);
        Debug.Log(this.Name + " has eaten: " + otherAgent.getName());
        if (other.gameObject.tag == "Enemy")
        {
          GameManager.RemoveEnemy(other.gameObject.GetInstanceID());
		  GameObject spawner = GameObject.FindWithTag("Spawner");
		  spawner.GetComponent<PowerUpSpawn>().DeathGeneratedPowerUp();
		  spawner.GetComponent<ProceduralSpawner>().enemyCount -= 1;
        }
        FindObjectOfType<AudioManager>().CreateAndPlay(this.gameObject, "EnemyEaten");
        if (other.gameObject.tag == "Player")
        {
          this.killCount += 1;
          GameManager.EndGameForPlayer();
        }
        else
        {
          Destroy(other.transform.parent.gameObject);
          this.killCount += 1;
        }
      }
    }
    else if (other.gameObject.tag == "PowerUp")
    {
	  GameObject spawner = GameObject.FindWithTag("Spawner");
	  spawner.GetComponent<ProceduralSpawner>().powerUpCount -= 1;
      this.powerUpsCollected += 1;
    }
  }

  // Function to update radius
  public virtual void UpdateRadius()
  {
    this.Radius = (float)Mathf.Pow(this.Score, 1f / 4f);
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

    this.previousLocalScale = transform.localScale;
    UpdateRadius();

    // Trigger size update event and set goal orbit
    this.goalLocalScale = new Vector3(1f, 1f, 1f) * this.Radius;
    this.sizeUpdateStartTime = Time.time;
    this.sizeUpdateTriggered = true;

    Leaderboard.Instance.SortLeaderboard();
  }

  // Function to update size
  public void UpdateSize()
  {
    // Prevents constant need to lerp scale if the size change is small
    if (this.sizeUpdateTriggered)
    {
      float t = (Time.time - this.sizeUpdateStartTime) / this.sizeUpdateDuration;
      transform.localScale = Vector3.Lerp(this.previousLocalScale, this.goalLocalScale, t);

      if (t >= 1f)
      {
        this.sizeUpdateTriggered = false;
        this.previousLocalScale = new Vector3(1f, 1f, 1f) * this.Radius;
      }
    }
  }

  // Function to implement score decay
  public void DecayScore()
  {
    if (this.InvincibilityMode)
    {
      // Exit early if the agent is invincible
      return;
    }

    this.decayTimer += Time.deltaTime; // increase timer
    float X = 1000f;
    float decayPerSecondAtX = 1f;
    this.decayDelta = X / (decayPerSecondAtX * this.ScoreDecayMultiplier * this.Score); // time between unit decay
    int reductionAmount = -1;

    if (this.decayDelta <= Time.deltaTime) // only called when score decay outpaces fps
    {
      // track decay loss using an excess tracker and add every integer of decay to reduction
      this.decayExcess += Time.deltaTime / this.decayDelta - 1;
      if (this.decayExcess >= 1)
      {
        reductionAmount -= (int)Mathf.Floor(this.decayExcess);
        this.decayExcess = 0f;
      }
    }

    if (this.Score > 1 && this.decayTimer >= this.decayDelta)
    {
      // Reduce score by one scaled unit and reset timer
      // UpdateScore(reductionAmount);
      this.Score += reductionAmount; // saves computational time
      this.decayTimer = 0f;
    }
  }

  // Function to generate random starting genetics
  public void GenerateRandomGenetics()
  {
    float geneticGrowthMin = 0.1f;
    float geneticGrowth = Mathf.Abs(normalRandom(0.5f, 0.1f)); // mean: 0.5, std: 0.1
    this.GeneticGrowthMultiplier = Mathf.Clamp(geneticGrowth, geneticGrowthMin, 1f);

    float foodGrowthMin = 1f;
    this.FoodGrowthMultiplier = Mathf.Max(foodGrowthMin, Mathf.Abs(normalRandom(0f, 1f))); // mean: 0, std: 1

    float speedMultMin = 1f;
    this.SpeedMultiplier = Mathf.Max(speedMultMin, Mathf.Abs(normalRandom(0f, 1f))); // mean: 1, std: 1

    float scoreDecayMax = 2f;
    this.ScoreDecayMultiplier = Mathf.Min(scoreDecayMax, Mathf.Abs(normalRandom(1f, 0.5f))); // mean: 1, std: 0.2

    float lockOnRadiusMin = 15f;
    this.LockOnRadiusMultiplier = Mathf.Max(lockOnRadiusMin, Mathf.Abs(normalRandom(30f, 5f))); // mean: 30, std: 5
  }

  // Function to take on superior genetics of eaten agent
  public void AssimilateGenetics(IntelligentAgent prey)
  {
    // Genetic growth multiplier, always first
    if (prey.getGeneticGrowthMultiplier() > this.GeneticGrowthMultiplier)
    {
      // Lerp to the new genetic growth multiplier based on the current
      this.GeneticGrowthMultiplier = Mathf.Lerp(this.GeneticGrowthMultiplier, prey.getGeneticGrowthMultiplier(), this.GeneticGrowthMultiplier);
    }

    // Food growth multiplier
    if (prey.getFoodGrowthMultiplier() > this.FoodGrowthMultiplier) // higher = better
    {
      this.FoodGrowthMultiplier = Mathf.Lerp(this.FoodGrowthMultiplier, prey.getFoodGrowthMultiplier(), this.GeneticGrowthMultiplier);
    }
    else
    {
      this.FoodGrowthMultiplier += 0.02f;
    }
    // Score decay multiplier
    if (prey.getScoreDecayMultiplier() < this.ScoreDecayMultiplier) // lower = better
    {
      this.ScoreDecayMultiplier = Mathf.Lerp(this.ScoreDecayMultiplier, prey.getScoreDecayMultiplier(), this.GeneticGrowthMultiplier);
    }
    else
    {
      this.ScoreDecayMultiplier *= 0.98f;
    }
    // Speed multiplier
    if (prey.getSpeedMultiplier() > this.SpeedMultiplier) // higher = better, unless you suck at flying like I do
    {
      this.SpeedMultiplier = Mathf.Lerp(this.SpeedMultiplier, prey.getSpeedMultiplier(), this.GeneticGrowthMultiplier);
    }
    else
    {
      this.SpeedMultiplier += 0.01f;
    }
    // Lock-on radius multiplier
    if (prey.getLockOnRadiusMultiplier() > this.LockOnRadiusMultiplier) // higher = better
    {
      this.LockOnRadiusMultiplier = Mathf.Lerp(this.LockOnRadiusMultiplier, prey.getLockOnRadiusMultiplier(), this.GeneticGrowthMultiplier);
    }
    else
    {
      this.LockOnRadiusMultiplier += 0.1f;
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
    float mySpeed = this.SpeedMultiplier * this.PowerUpSpeedMultiplier / transform.localScale.x;
    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f * mySpeed);
  }

  // Find nearest object in a dictionary
  public GameObject GetClosestObject(Dictionary<int, GameObject> objs)
  {
    GameObject closest = null;
    float minDist = Mathf.Infinity;
    Vector3 currentPos = this.transform.position;
    float epsilon = 1e-4f;

    foreach (KeyValuePair<int, GameObject> clone in objs)
    {
      if (clone.Value.GetComponent<MeshRenderer>().enabled) // only rendered objects will be considered
      {
        Vector3 directionToObject = clone.Value.transform.position - currentPos;
        float distSqrToTarget = directionToObject.sqrMagnitude;

        if (distSqrToTarget < minDist && distSqrToTarget > epsilon)
        {
          closest = clone.Value;
          minDist = distSqrToTarget;
        }
      }
    }
    return closest;
  }

  // Setter method for position
  public void setPosition(Vector3 pos)
  {
    transform.position = pos;
  }

  // Setter method for Power Up speed multiplier
  public void setPowerUpSpeedMultiplier(float newMult)
  {
    this.PowerUpSpeedMultiplier = newMult;
  }

  // Setter method for Power Up invin growth multiplier
  public void setInvinGrowthMultiplier(float newMult)
  {
    this.InvinGrowthMultiplier = newMult;
  }

  // Function to set the target of the agent
  public virtual void setTarget(GameObject obj)
  {
    this.Target = obj;
  }

  //Function to get the target of the agent
  public GameObject getTarget()
  {
    return this.Target;
  }

  // Setter method for name
  public void setName(string name)
  {
    this.Name = name;
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

  // Getter method for lock-on radius multiplier
  public float getLockOnRadiusMultiplier()
  {
    return this.LockOnRadiusMultiplier;
  }

  // Setter method for lock-on rafius multipler
  public void setLockOnRadiusMultiplier(float newMult)
  {
    this.LockOnRadiusMultiplier = newMult;
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

  // Setter method for speed multiplier
  public void setSpeedMultiplier(float newMult)
  {
    this.SpeedMultiplier = newMult;
  }

  // Getter method for genetic growth multiplier
  public float getGeneticGrowthMultiplier()
  {
    return this.GeneticGrowthMultiplier;
  }

  // Getter method for food growth multiplier
  public float getFoodGrowthMultiplier()
  {
    return this.FoodGrowthMultiplier;
  }

  // Setter method for food growth multiplier
  public void setFoodGrowthMultiplier(float newMult)
  {
    this.FoodGrowthMultiplier = newMult;
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

  // Getter method for size update event start time
  public float getSizeUpdateDuration()
  {
    return this.sizeUpdateDuration;
  }

  // Function to set activePowerUps
  public void setActivePowers(int num)
  {
    this.activePowers = num;
  }

  // Function to get the number of activePowers of the agent
  public int getActivePowers()
  {
    return this.activePowers;
  }

  
  // Function to check if agent is invincible
  public bool isInvincible()
  {
    return this.InvincibilityMode;
  }

  // Function to check how many invincible power-ups
  public void setActiveInvin(int num){
    this.activeInvin = num;
    if(num > 0){
      this.InvincibilityMode = true;
    }else{
      this.InvincibilityMode = false;
    }
  }

  public int getActiveInvin(){
    return this.activeInvin;
  }


  // Function to check if agent has speed power-up
  public bool isSpeed()
  {
    return this.SpeedMode;
  }

  // Function to check how many speed power-ups
  public void setActiveSpeed(int num){
    this.activeSpeed = num;
    if(num > 0){
      this.SpeedMode = true;
    }else{
      this.SpeedMode = false;
    }
  }

  public int getActiveSpeed(){
    return this.activeSpeed;
  }

  // Function to check if agent is Magnet
  public bool isMagnet()
  {
    return this.MagnetMode;
  }

  // Function to check how many Magnet power-ups
  public void setActiveMagnet(int num){
    this.activeMagnet = num;
    if(num > 0){
      this.MagnetMode = true;
    }else{
      this.MagnetMode = false;
    }
  }

  public int getActiveMagnet(){
    return this.activeMagnet;
  }
  
}



