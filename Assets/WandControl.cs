using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class WandControl : MonoBehaviour {

	public GameObject wordmaker;

	Dictionary<string,AudioClip> wordclips = new Dictionary<string,AudioClip>();

	makeaword wordmakerScript = null;
	Vector3 hydrapos;
	Vector3 wordpos;
	Quaternion hydrarot = new Quaternion();
	bool makeword1 = false;

	// Use this for initialization
	void Start () {
		wordclips.Add ("yes", Resources.Load ("Yes - Maria") as AudioClip);
		wordclips.Add ("no", Resources.Load ("No - Maria") as AudioClip);

		wordmakerScript = wordmaker.GetComponent<MonoBehaviour>() as makeaword;
		SocketDispatch.On (Google.Protobuf.VRCom.Update.VrmsgOneofCase.Hydra, handleHydra);
	}

	// Update is called once per frame
	void Update () {

	}

	void handleHydra(Google.Protobuf.VRCom.Update msg) {
		if (msg.Hydra.CtrlNum == 0) {
			hydrapos.Set (msg.Hydra.Pos.X/1000, msg.Hydra.Pos.Y/1000, -msg.Hydra.Pos.Z/1000);
			hydrarot.Set (msg.Hydra.Rot.X, msg.Hydra.Rot.Y, -msg.Hydra.Rot.Z, -msg.Hydra.Rot.W);
			transform.localPosition = hydrapos;
			transform.localRotation = hydrarot;

			if ((msg.Hydra.Buttons & ControllerButtons.SIXENSE_BUTTON_1) != 0) {
				if (!makeword1) {
					makeword1 = true;
				}
			} else if (makeword1) {
				makeword1 = false;
				wordpos = transform.position+transform.forward*.30f;
				wordmakerScript.makeword ("yes", 100f, wordpos, transform.rotation, wordclips["yes"]);
			}
		}
	}
}
