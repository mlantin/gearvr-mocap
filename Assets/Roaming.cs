using UnityEngine;
using System.Collections;

public class Roaming : MonoBehaviour {

	Vector3 newpos = new Vector3();
	Quaternion newrot = new Quaternion();
	OVRPose headpos = new OVRPose();
	bool trackingSpaceSet = false;
	bool havemocap = false;

	void Awake() {
		var rig = GameObject.FindObjectOfType<OVRCameraRig>();
		//rig.UpdatedAnchors += r => r.trackingSpace.FromOVRPose(headpos*rig.centerEyeAnchor.ToOVRPose(true).Inverse());
		rig.UpdatedAnchors += UpdatedAnchors;

	}

	// Use this for initialization
	void Start () {
		SocketDispatch.On ("GVR", handleMocap);
	}


	void UpdatedAnchors(OVRCameraRig r) {

//		if (!trackingSpaceSet && havemocap) {
//			r.trackingSpace.transform.rotation = newrot;
//			r.trackingSpace.transform.position = newpos;
//			OVRManager.display.RecenterPose ();
//			trackingSpaceSet = true;
//		}
		Quaternion hmd_rotation = Quaternion.identity;

		Quaternion oldOrientation = headpos.orientation;
		hmd_rotation = r.centerEyeAnchor.ToOVRPose (true).orientation;
		//hmd_rotation = OVRManager.display.GetHeadPose(0).orientation;
		headpos.orientation = Quaternion.Slerp(oldOrientation, newrot * Quaternion.Inverse(hmd_rotation), Time.deltaTime);
		headpos.position = newpos;
		//r.trackingSpace.FromOVRPose(headpos);
		transform.rotation = headpos.orientation;
		//transform.position = headpos.position;
	}

	// Update is called once per frame
	void Update () {
		transform.position = newpos;
		//transform.localRotation = newrot;
	}

	void handleMocap(Google.Protobuf.VRCom.MocapSubject head) {
		// the data coming in is OpenGL convention, X Right, Y UP, Z Backward
		// Unity is the same but with Z pointing forward.
		havemocap = true;
		newpos.Set (head.Pos.X/1000, head.Pos.Y/1000, -head.Pos.Z/1000);
		newrot.Set (head.Rot.X, head.Rot.Y, -head.Rot.Z, -head.Rot.W);
	}
}
