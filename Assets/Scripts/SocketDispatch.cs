using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WiimoteButtons {
	static uint WIIMOTE_BUTTON_TWO	= 0x0001;
	static uint WIIMOTE_BUTTON_ONE	= 0x0002;
	static uint WIIMOTE_BUTTON_B	= 0x0004;
	static uint WIIMOTE_BUTTON_A	= 0x0008;
	static uint WIIMOTE_BUTTON_MINUS = 0x0010;
	static uint WIIMOTE_BUTTON_ZACCEL_BIT6 = 0x0020;
	static uint WIIMOTE_BUTTON_ZACCEL_BIT7 = 0x0040;
	static uint WIIMOTE_BUTTON_HOME = 0x0080;
	static uint WIIMOTE_BUTTON_LEFT = 0x0100;
	static uint WIIMOTE_BUTTON_RIGHT	= 0x0200;
	static uint WIIMOTE_BUTTON_DOWN = 0x0400;
	static uint WIIMOTE_BUTTON_UP = 0x0800;
	static uint WIIMOTE_BUTTON_PLUS = 0x1000;
	static uint WIIMOTE_BUTTON_ZACCEL_BIT4 = 0x2000;
	static uint WIIMOTE_BUTTON_ZACCEL_BIT5 = 0x4000;
	static uint WIIMOTE_BUTTON_UNKNOWN	= 0x8000;
	static uint WIIMOTE_BUTTON_ALL = 0x1F9F;
};

public class SocketDispatch : MonoBehaviour {

	public delegate void VRMsgHandler(Google.Protobuf.VRCom.Update msg);
	public delegate void MocapHandler(Google.Protobuf.VRCom.MocapSubject msg);

	static event VRMsgHandler OnMocapMsg;
	static event VRMsgHandler OnHydraMsg;
	static event VRMsgHandler OnWiimoteMsg;

	static Dictionary <string, MocapHandler> mocapHandlers = new Dictionary<string,MocapHandler>();

	public string address = "ws://127.0.0.1:4567";
	WebSocket w;
	Google.Protobuf.VRCom.Update updateMsg = new Google.Protobuf.VRCom.Update();


	// Use this for initialization
	IEnumerator Start () {
		w = new WebSocket(new Uri(address));
		yield return StartCoroutine(w.Connect());
		w.SendString ("{ \"username\":\"" + SystemInfo.deviceUniqueIdentifier + "\"}" );
		while (true)
		{
			WebSocketSharp.MessageEventArgs msg = w.Recv();
			if (msg != null)
			{
				if (msg.Type == WebSocketSharp.Opcode.Binary) {
					updateMsg.ClearVrmsg ();
					updateMsg.MergeFrom (new Google.Protobuf.CodedInputStream(msg.RawData));
					Google.Protobuf.VRCom.Update.VrmsgOneofCase msgType = updateMsg.VrmsgCase;
					switch (msgType) {
					case Google.Protobuf.VRCom.Update.VrmsgOneofCase.Mocap:
						if (OnMocapMsg != null)
							OnMocapMsg (updateMsg);
						Google.Protobuf.Collections.MapField<string, Google.Protobuf.VRCom.MocapSubject> subjects = updateMsg.Mocap.Subjects;
						foreach (KeyValuePair<string,MocapHandler> pair in mocapHandlers) {
							if (subjects.ContainsKey(pair.Key))
								mocapHandlers[pair.Key](subjects[pair.Key]);
						}
						break;
					case Google.Protobuf.VRCom.Update.VrmsgOneofCase.Hydra:
						if (OnHydraMsg != null)
							OnHydraMsg (updateMsg);
						break;
					case Google.Protobuf.VRCom.Update.VrmsgOneofCase.Wiimote:
						if (OnWiimoteMsg != null)
							OnWiimoteMsg (updateMsg);
						break;
					default:
						Debug.Log ("Received an unknown or empty message");
						break;
					}
				}
			}
			if (w.error != null)
			{
				Debug.LogError ("Error: "+w.error);
				break;
			}
			yield return 0;
		}
		w.Close();
	}
	
	// Update is called once per frame
	void Update () {
	}
		
	public static void On(Google.Protobuf.VRCom.Update.VrmsgOneofCase type, VRMsgHandler handler) {
		switch(type) {
		case Google.Protobuf.VRCom.Update.VrmsgOneofCase.Mocap:
			OnMocapMsg += handler;
			break;
		case Google.Protobuf.VRCom.Update.VrmsgOneofCase.Hydra:
			OnHydraMsg += handler;
			break;
		case Google.Protobuf.VRCom.Update.VrmsgOneofCase.Wiimote:
			OnWiimoteMsg += handler;
			break;
		default:
			break;
		}
	}

	public static void On(string subjectName, MocapHandler handler) {
		if (mocapHandlers.ContainsKey (subjectName))
			mocapHandlers [subjectName] += handler;
		else
			mocapHandlers [subjectName] = handler;
	}

	void OnApplicationQuit() {
		Debug.Log (w.m_Messages.Count);
		w.Close ();
	}
}
