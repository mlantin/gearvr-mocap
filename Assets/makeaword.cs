using UnityEngine;
using System.Collections;

public class makeaword : MonoBehaviour {

	public GameObject alphabet;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Y)) {
			Vector3 pos = new Vector3 (0f, 1f, 1f);
			makeword("yes",100f,pos,Quaternion.identity);
			AudioSource audio = GetComponent<AudioSource>();
			audio.Play();
		}
	}

	void makeword(string word, float scale, Vector3 pos, Quaternion rot) {
		GameObject newword = new GameObject (word);
		newword.transform.parent = transform;

		GameObject newletter;
		MeshFilter lettermesh;
		Vector3 letterpos = new Vector3 ();
		Vector3 scalevec = new Vector3 (scale, scale, scale);
		Vector3 letterscale = new Vector3 (1f, 1f, 1f);

		newword.transform.localScale = scalevec;
		newword.transform.localPosition = pos;
		newword.transform.localRotation = rot;

		MeshFilter[] letters = alphabet.GetComponentsInChildren<MeshFilter> ();
		Vector3 lettercentre;
		Vector3 extent;
		foreach (char c in word) {
			foreach (MeshFilter letter in letters) {
				if (letter.name == c.ToString()) {
					lettermesh = Instantiate(letter) as MeshFilter;
					newletter = lettermesh.gameObject;
					newletter.name = c.ToString ();
					newletter.transform.parent = newword.transform;
					newletter.transform.localScale = letterscale;
					lettercentre = letter.sharedMesh.bounds.center;
					extent = letter.sharedMesh.bounds.extents;
					newletter.transform.localPosition = letterpos-lettercentre;
					letterpos.x += extent.x*2;
					break;
				}
			}
		}
	}
}
