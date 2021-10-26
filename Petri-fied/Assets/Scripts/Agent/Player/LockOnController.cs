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
	
    // Start is called before the first frame update
    void Start()
    {
		adaptedMaterial = new Material(lockOnPrefab);
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
			// Return early on new target being null
			return;
		}
		else
		{
			lockOnOriginal = new Material(newTarget.gameObject.GetComponent<Renderer>().material);
			Color newMainColour;
			if (lockOnOriginal.HasProperty("_MainColor"))
			{
				newMainColour = lockOnOriginal.GetColor("_MainColor");
			}
			else
			{
				newMainColour = lockOnOriginal.color;
			}
			adaptedMaterial.SetColor("_MainColor", newMainColour);
			
			if (newTarget.gameObject.tag == "Enemy")
			{
				float targetScore = newTarget.GetComponent<IntelligentAgent>().getScore();
				float playerScore = GetComponent<Player>().getScore();
				Color oldGlowColour = this.lockOnPrefab.GetColor("_OutlineColor");
				if (targetScore >= playerScore)
				{
					Color newGlowColour = new Color32(145, 0, 8, 255); // manually picked, not fussed
					adaptedMaterial.SetColor("_OutlineColor", newGlowColour);
				}
				else
				{
					adaptedMaterial.SetColor("_OutlineColor", oldGlowColour);
				}
			}
			newTarget.gameObject.GetComponent<Renderer>().material = adaptedMaterial;
		}
	}
	
	// Function to revert replaced target's material back to original
	void RevertTargetMaterial()
	{
		// Reset the target's material and potentially changed lock-on outline colour
		this.CurrentTarget.gameObject.GetComponent<Renderer>().material = lockOnOriginal;
		Color oldGlowColour = this.lockOnPrefab.GetColor("_OutlineColor");
		adaptedMaterial.SetColor("_OutlineColor", oldGlowColour);
	}
}
