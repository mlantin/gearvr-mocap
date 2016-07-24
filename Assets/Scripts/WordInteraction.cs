using UnityEngine;
using System.Collections;

public class WordInteraction : MonoBehaviour {

	OSCtoMax maxsender;

	void Start() {
		maxsender = GetComponent<OSCtoMax>();
	}

	void OnCollisionEnter (Collision col) {
		Debug.Log ("collided! with " + col.gameObject.name + " at ");
		foreach (ContactPoint contact in col.contacts) {
			Vector3 localcontact = col.gameObject.transform.InverseTransformPoint (contact.point);
			Debug.Log (localcontact.x / (col.collider.bounds.extents.x*2f/col.gameObject.transform.localScale.x)+ " " + localcontact.y + localcontact.z);
		}
		//maxsender.MySendOSCMessageTriggerMethod ();
	}
}
