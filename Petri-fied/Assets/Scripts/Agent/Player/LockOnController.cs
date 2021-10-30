using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnController : MonoBehaviour
{
	// Current player lock-on target
	[SerializeField] private GameObject CurrentTarget = null;
	
	// Lock-on target original material and adaptable lock-on material
	public Material lockOnPrefab;
	private Material lockOnOriginal;
	private Material adaptedMaterial;
	private Color deadlyEnemyGlow; // manually picked in start, not fussed
	private Color weakEnemyGlow;
	private bool enemyLocked = false;
	
    // Start is called before the first frame update
    void Start()
    {
		this.adaptedMaterial = new Material(this.lockOnPrefab);
		this.weakEnemyGlow = this.lockOnPrefab.GetColor("_OutlineColor");
		this.deadlyEnemyGlow = new Color32(145, 0, 8, 255);
    }
	
	// Called at the end of every frame
	void LateUpdate()
	{
		// Exit early if current target no longer exists (might've been eaten)
		if (this.CurrentTarget == null)
		{
			this.enemyLocked = false;
			return;
		}
		// Now check if the target is still being renderer (applies to power ups that got eaten)
		if (!this.CurrentTarget.GetComponent<MeshRenderer>().enabled)
		{
			GetComponent<IntelligentAgent>().setTarget(null);
			return;
		}
		// Otherwise, check if target is an enemy and assign material from score difference
		if (this.enemyLocked)
		{
			float targetScore = CurrentTarget.GetComponent<IntelligentAgent>().getScore();
			float playerScore = GetComponent<Player>().getScore();
			
			if (targetScore >= playerScore)
			{
				this.adaptedMaterial.SetColor("_OutlineColor", this.deadlyEnemyGlow);
			}
			else
			{
				this.adaptedMaterial.SetColor("_OutlineColor", this.weakEnemyGlow);
			}
			this.CurrentTarget.gameObject.GetComponent<Renderer>().material = this.adaptedMaterial;
		}
	}
	
	// Function to chaneg lock-on target's material colour at runtime
	public void UpdateTargetMaterial(GameObject newTarget)
	{
		if (this.CurrentTarget != null)
		{
			// An update is occuring, therefore revert previous target's material
			RevertTargetMaterial();
		}
		
		// Update current target
		this.CurrentTarget = newTarget;
		
		if (newTarget == null)
		{
			// Return early after reseting enemy locked onto status
			this.enemyLocked = false;
			return;
		}
		else
		{
			lockOnOriginal = new Material(newTarget.gameObject.GetComponent<Renderer>().material);
			Color newMainColour;
			if (this.lockOnOriginal.HasProperty("_MainColor"))
			{
				newMainColour = this.lockOnOriginal.GetColor("_MainColor");
			}
			else
			{
				newMainColour = this.lockOnOriginal.color;
			}
			this.adaptedMaterial.SetColor("_MainColor", newMainColour);
			
			if (newTarget.gameObject.tag == "Enemy")
			{
				this.enemyLocked = true;
				float targetScore = newTarget.GetComponent<IntelligentAgent>().getScore();
				float playerScore = GetComponent<Player>().getScore();
				if (targetScore >= playerScore)
				{
					this.adaptedMaterial.SetColor("_OutlineColor", this.deadlyEnemyGlow);
				}
				else
				{
					this.adaptedMaterial.SetColor("_OutlineColor", this.weakEnemyGlow);
				}
			}
			else
			{
				this.enemyLocked = false;
			}
			newTarget.gameObject.GetComponent<Renderer>().material = this.adaptedMaterial;
		}
	}
	
	// Function to revert replaced target's material back to original
	void RevertTargetMaterial()
	{
		// Reset the target's material and potentially changed lock-on outline colour
		this.CurrentTarget.gameObject.GetComponent<Renderer>().material = this.lockOnOriginal;
		Color oldGlowColour = this.lockOnPrefab.GetColor("_OutlineColor");
		this.adaptedMaterial.SetColor("_OutlineColor", this.weakEnemyGlow); // reset the adapted outline colour
	}
}
