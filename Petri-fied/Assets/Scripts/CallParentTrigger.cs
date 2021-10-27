using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallParentTrigger : MonoBehaviour
{
	// Function to call parent on trigger enter
	void OnTriggerEnter(Collider other)
	{
		gameObject.GetComponentInParent<IntelligentAgent>().ParentOnTriggerEnter(other);
	}
}
