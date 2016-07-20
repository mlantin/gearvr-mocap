using UnityEngine;
using System.Collections;

public class WordInteraction : MonoBehaviour {

	void OnCollisionEnter (Collision col){
		if (col.gameObject.name == "wand") {
			Debug.Log ("collided! with " + name);
		}
	}
}
