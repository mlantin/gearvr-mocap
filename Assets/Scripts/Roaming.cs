using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Roaming : MonoBehaviour {

	public GameObject worldOrigin;

	Vector3 newpos = new Vector3(0f,1.6f,0f);
	Quaternion newrot = new Quaternion();
	bool havemocap = false;
	bool trackingspaceSet = false;

	void Awake() {
		var rig = GameObject.FindObjectOfType<OVRCameraRig>();
		rig.UpdatedAnchors += UpdatedAnchors;
		OVRManager.HMDAcquired += () => trackingspaceSet = false;
		OVRManager.HMDLost += () => trackingspaceSet = false;
	}

	// Use this for initialization
	void Start () {
		SocketDispatch.On ("GVR2", handleMocap);
	}


	void UpdatedAnchors(OVRCameraRig r) {
		Quaternion hmd_rotation = Quaternion.identity;
		if (havemocap) {
			// get the difference between the hmd rotation and the world origin rotation.
			hmd_rotation = r.centerEyeAnchor.ToOVRPose (true).orientation;
			Quaternion rotdiff = hmd_rotation*Quaternion.Inverse (newrot);
		
			// if we're just starting up, set the world orientation to match the gearvr
			if (!trackingspaceSet) {
				worldOrigin.transform.rotation = rotdiff;
				trackingspaceSet = true;
			} else {
				// otherwise adjust the world orientation up to .01 of a degree. Basically this will drift the world
				// orientation in tandem with the gearvr. Seems to work. I'm sure the value of .01 could be tweaked
				// to be optimal. The issue is that you don't want to have too large because then it can shift wildly
				// because of the mocap lag. There may be a way to check for moments of relative quiet and measure
				// the drift. In practice this code works ok but it does mean that the world is always a bit shifty
				// which probably induces nausea.
				worldOrigin.transform.rotation = Quaternion.RotateTowards (worldOrigin.transform.rotation, rotdiff, .01f);
			}
		}
			
		transform.position = worldOrigin.transform.rotation * newpos;
	}

	// Update is called once per frame
	void Update () {

	}

	void handleMocap(Google.Protobuf.VRCom.MocapSubject head) {
		Debug.Log ("mocap data");
		// the data coming in is OpenGL convention, X Right, Y UP, Z Backward
		// Unity is the same but with Z pointing forward.
		havemocap = true;
		// position is in mm for Vicon. In Unity we usually model with metres.
		newpos.Set (head.Pos.X/1000, head.Pos.Y/1000, -head.Pos.Z/1000);
		newrot.Set (head.Rot.X, head.Rot.Y, -head.Rot.Z, -head.Rot.W);
	}
}
