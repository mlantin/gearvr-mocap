using UnityEngine;
using System.Collections;

public class CubeMotion : MonoBehaviour {

	Vector3 newpos = new Vector3();
	Quaternion newrot = new Quaternion();

	// Use this for initialization
	void Start () {
		SocketDispatch.On ("BALL", handleMocap);
		SocketDispatch.On (Google.Protobuf.VRCom.Update.VrmsgOneofCase.Hydra, handleHydra);
	}
		
	// Update is called once per frame
	void Update () {
		transform.position = newpos;
		transform.rotation = newrot;
	}

	void handleMocap(Google.Protobuf.VRCom.MocapSubject msg) {
		// the data coming in from the websocket is OpenGL convention, X Right, Y UP, Z Backward
		// Unity is the same but with Z pointing forward. So we are mirroring about the XY plane.
		newpos.Set (msg.Pos.X/1000, msg.Pos.Y/1000, -msg.Pos.Z/1000);
		newrot.Set (msg.Rot.X, msg.Rot.Y, -msg.Rot.Z, -msg.Rot.Z);

	}

	void handleHydra(Google.Protobuf.VRCom.Update msg) {
		if (msg.Hydra.CtrlNum == 0) {
			newrot.Set (msg.Hydra.Rot.X, msg.Hydra.Rot.Y, -msg.Hydra.Rot.Z, -msg.Hydra.Rot.W);
			newpos.Set(msg.Hydra.Pos.X/1000, msg.Hydra.Pos.Y/1000, -msg.Hydra.Pos.Z/1000);
		}
	}
}
