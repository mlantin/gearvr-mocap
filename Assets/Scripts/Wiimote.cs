using UnityEngine;
using System.Collections;

public class Wiimote : MonoBehaviour {

	// Use this for initialization
	void Start () {
		SocketDispatch.On (Google.Protobuf.VRCom.Update.VrmsgOneofCase.Wiimote, handleWiimote);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void handleWiimote(Google.Protobuf.VRCom.Update msg) {
		Google.Protobuf.VRCom.Wiimote wmsg = msg.Wiimote;
		Debug.Log (wmsg.ToString());
		if (wmsg.ButtonsPressed != 0)
			Debug.Log ("a button was pressed "+wmsg.ButtonsPressed);
		if (wmsg.ButtonsReleased != 0)
			Debug.Log ("a button was released "+wmsg.ButtonsReleased);
	}
}
