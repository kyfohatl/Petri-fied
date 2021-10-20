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
			
			Vector4 newColour;
			if (lockOnOriginal.HasProperty("_MainColor"))
			{
				newColour = lockOnOriginal.GetColor("_MainColor");
			}
			else
			{
				newColour = lockOnOriginal.color;
			}
			adaptedMaterial.SetColor("_MainColor", newColour);
			newTarget.gameObject.GetComponent<Renderer>().material = adaptedMaterial;
		}
	}
	
	// Function to revert replaced target's material back to original
	void RevertTargetMaterial()
	{
		this.CurrentTarget.gameObject.GetComponent<Renderer>().material = lockOnOriginal;
	}
}
