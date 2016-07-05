﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Roaming : MonoBehaviour {

	public GameObject worldOrigin;
	public Text debugTxt;

	Vector3 newpos = new Vector3();
	Quaternion newrot = new Quaternion();
	OVRPose headpos = new OVRPose();
	bool havemocap = false;
	bool trackingspaceSet = false;

	void Awake() {
		var rig = GameObject.FindObjectOfType<OVRCameraRig>();
		//rig.UpdatedAnchors += r => r.trackingSpace.FromOVRPose(headpos*rig.centerEyeAnchor.ToOVRPose(true).Inverse());
		rig.UpdatedAnchors += UpdatedAnchors;

	}

	// Use this for initialization
	void Start () {
		SocketDispatch.On ("GVR2", handleMocap);
	}


	void UpdatedAnchors(OVRCameraRig r) {
		Quaternion hmd_rotation = Quaternion.identity;
		if (havemocap && !trackingspaceSet) {
			// get the difference between the hmd rotation and the world origin rotation.
			OVRPlugin.Quatf hmd_origin = OVRPlugin.GetTrackingCalibratedOrigin().Orientation;
			Quaternion hmdOrigin = new Quaternion (hmd_origin.x, hmd_origin.y, hmd_origin.z, hmd_origin.w);
			debugTxt.text = newrot.eulerAngles.ToString ();
			hmd_rotation = r.centerEyeAnchor.ToOVRPose (true).orientation;
			Quaternion rotdiff = Quaternion.Inverse (newrot)*hmdOrigin;

			// now slerp the world origin to make up for the difference between the hmd and mocap orientation
			// at the start this will mean a slight delay in setting the world orientation because there will be
			// a significant difference between the mocap and the hmd. Maybe 1 or 2 seconds.
			// could also try using the rotateToward function.
			worldOrigin.transform.rotation = rotdiff;
			trackingspaceSet = true;
		}


//		Quaternion oldOrientation = headpos.orientation;
//		hmd_rotation = r.centerEyeAnchor.ToOVRPose (true).orientation;
		//hmd_rotation = OVRManager.display.GetHeadPose(0).orientation;
//		headpos.orientation = Quaternion.Slerp(oldOrientation, newrot * Quaternion.Inverse(hmd_rotation), Time.deltaTime);
		headpos.position = newpos;
		//r.trackingSpace.FromOVRPose(headpos);
//		transform.rotation = headpos.orientation;
		transform.position = worldOrigin.transform.rotation * headpos.position;
	}

	// Update is called once per frame
	void Update () {
		//transform.position = newpos;
		//transform.localRotation = newrot;
		debugTxt.text = newrot.eulerAngles.ToString ();
	}

	void handleMocap(Google.Protobuf.VRCom.MocapSubject head) {
		// the data coming in is OpenGL convention, X Right, Y UP, Z Backward
		// Unity is the same but with Z pointing forward.
		havemocap = true;
		// position is in mm for Vicon. In Unity we usually model with metres.
		newpos.Set (head.Pos.X/1000, head.Pos.Y/1000, -head.Pos.Z/1000);
		newrot.Set (head.Rot.X, head.Rot.Y, -head.Rot.Z, -head.Rot.W);
	}
}
