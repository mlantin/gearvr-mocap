using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class WandControl : MonoBehaviour {

	public GameObject wordmaker;

	Dictionary<string,AudioClip> wordclips = new Dictionary<string,AudioClip>();

	makeaword wordmakerScript = null;
	Vector3 wandpos;
	Vector3 wordpos;
	Quaternion wandrot = new Quaternion();
	bool makeword1 = false;

	// Use this for initialization
	void Start () {
		wordclips.Add ("yes", Resources.Load ("Yes - Maria") as AudioClip);
		wordclips.Add ("no", Resources.Load ("No - Maria") as AudioClip);

		wordmakerScript = wordmaker.GetComponent<MonoBehaviour>() as makeaword;
		SocketDispatch.On (Google.Protobuf.VRCom.Update.VrmsgOneofCase.Hydra, handleHydra);
		SocketDispatch.On (Google.Protobuf.VRCom.Update.VrmsgOneofCase.Wiimote, handleWii);
		SocketDispatch.On ("Wii", handleWiiMocap);
	}

	// Update is called once per frame
	void Update () {
		transform.localPosition = wandpos;
		transform.localRotation = wandrot;
		wordpos = transform.position+transform.up*.30f;
	}

	void handleHydra(Google.Protobuf.VRCom.Update msg) {
		if (msg.Hydra.CtrlNum == 0) {
			wandpos.Set (msg.Hydra.Pos.X/1000, msg.Hydra.Pos.Y/1000, msg.Hydra.Pos.Z/1000);
			wandrot.Set (msg.Hydra.Rot.X, msg.Hydra.Rot.Y, -msg.Hydra.Rot.Z, -msg.Hydra.Rot.W);
			transform.localPosition = wandpos;
			transform.localRotation = wandrot;

			if ((msg.Hydra.Buttons & ControllerButtons.SIXENSE_BUTTON_1) != 0) {
				if (!makeword1) {
					makeword1 = true;
				}
			} else if (makeword1) {
				makeword1 = false;
				wordpos = transform.position+transform.forward*.30f;
				wordmakerScript.makeword ("yes", 0.1f, wordpos, transform.rotation, wordclips["yes"]);
			}
		}
	}

	void handleWii(Google.Protobuf.VRCom.Update msg) {
		if ((msg.Wiimote.ButtonsPressed & ControllerButtons.WIIMOTE_BUTTON_DOWN) != 0) {
			wordmakerScript.makeword ("no", 0.1f, wordpos,Quaternion.AngleAxis(90,new Vector3(0,1,0)), wordclips ["no"]);
		} else if ((msg.Wiimote.ButtonsPressed & ControllerButtons.WIIMOTE_BUTTON_UP) != 0) {
			wordmakerScript.makeword ("yes", 0.1f, wordpos, transform.rotation*Quaternion.AngleAxis(90,new Vector3(0,1,0)), wordclips ["yes"]);
		}
	}

	void handleWiiMocap(Google.Protobuf.VRCom.MocapSubject wii) {
		wandpos.Set (wii.Pos.X / 1000, wii.Pos.Y / 1000, -wii.Pos.Z / 1000);
		wandrot.Set (wii.Rot.X, wii.Rot.Y, -wii.Rot.Z, -wii.Rot.W);
	}
}
