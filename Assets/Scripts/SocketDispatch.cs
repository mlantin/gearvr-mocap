using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ControllerButtons {
	public static uint WIIMOTE_BUTTON_TWO	= 0x0001;
	public static uint WIIMOTE_BUTTON_ONE	= 0x0002;
	public static uint WIIMOTE_BUTTON_B	= 0x0004;
	public static uint WIIMOTE_BUTTON_A	= 0x0008;
	public static uint WIIMOTE_BUTTON_MINUS = 0x0010;
	public static uint WIIMOTE_BUTTON_ZACCEL_BIT6 = 0x0020;
	public static uint WIIMOTE_BUTTON_ZACCEL_BIT7 = 0x0040;
	public static uint WIIMOTE_BUTTON_HOME = 0x0080;
	public static uint WIIMOTE_BUTTON_LEFT = 0x0100;
	public static uint WIIMOTE_BUTTON_RIGHT	= 0x0200;
	public static uint WIIMOTE_BUTTON_DOWN = 0x0400;
	public static uint WIIMOTE_BUTTON_UP = 0x0800;
	public static uint WIIMOTE_BUTTON_PLUS = 0x1000;
	public static uint WIIMOTE_BUTTON_ZACCEL_BIT4 = 0x2000;
	public static uint WIIMOTE_BUTTON_ZACCEL_BIT5 = 0x4000;
	public static uint WIIMOTE_BUTTON_UNKNOWN	= 0x8000;
	public static uint WIIMOTE_BUTTON_ALL = 0x1F9F;

	public static uint SIXENSE_BUTTON_BUMPER =  (0x01<<7);
	public static uint SIXENSE_BUTTON_JOYSTICK =(0x01<<8);
	public static uint SIXENSE_BUTTON_1   =     (0x01<<5);
	public static uint SIXENSE_BUTTON_2   =     (0x01<<6);
	public static uint SIXENSE_BUTTON_3   =     (0x01<<3);
	public static uint SIXENSE_BUTTON_4   =     (0x01<<4);
	public static uint SIXENSE_BUTTON_START =   (0x01<<0);

};

public class SocketDispatch : MonoBehaviour {

	public delegate void VRMsgHandler(Google.Protobuf.VRCom.Update msg);
	public delegate void MocapHandler(Google.Protobuf.VRCom.MocapSubject msg);

	static event VRMsgHandler OnMocapMsg;
	static event VRMsgHandler OnHydraMsg;
	static event VRMsgHandler OnWiimoteMsg;

	static Dictionary <string, MocapHandler> mocapHandlers = new Dictionary<string,MocapHandler>();

	public string address = "ws://127.0.0.1:4567";
	WebSocketSharp.WebSocket w;
	bool socketIsConnected = false;
	string socketError = null;

	// for reading the generic message
	Google.Protobuf.VRCom.Update updateMsg = new Google.Protobuf.VRCom.Update();
	// keep only the last mocap message received
	Google.Protobuf.VRCom.Update mocapMsg = null;
	// keep only the last hydra message received
	Google.Protobuf.VRCom.Update hydraMsg = null;
	// keep a queue of wiimote messages
	Queue<Google.Protobuf.VRCom.Update> wiimoteMsgs = new Queue<Google.Protobuf.VRCom.Update>();
	// Used in the dispatch loop
	Google.Protobuf.VRCom.Update currMsg = null;

	System.Object updateLock = new System.Object();

	// Use this for initialization
	IEnumerator Start () {
		
		yield return StartCoroutine(ConnectWebsocket());
		w.Send ("{ \"username\":\"" + SystemInfo.deviceUniqueIdentifier + "\"}" );
		while (socketError == null)
		{
			lock (updateLock) {
				if (mocapMsg != null) {
					if (OnMocapMsg != null)
						OnMocapMsg (mocapMsg);
					Google.Protobuf.Collections.MapField<string, Google.Protobuf.VRCom.MocapSubject> subjects = mocapMsg.Mocap.Subjects;
					foreach (KeyValuePair<string,MocapHandler> pair in mocapHandlers) {
						if (subjects.ContainsKey (pair.Key))
							mocapHandlers [pair.Key] (subjects [pair.Key]);
					}
					mocapMsg = null;
				}		
				if (hydraMsg != null) {
					if (OnHydraMsg != null)
						OnHydraMsg (hydraMsg);
					hydraMsg = null;
				}
				while (wiimoteMsgs.Count != 0) {
					if (OnWiimoteMsg != null)
						OnWiimoteMsg (wiimoteMsgs.Dequeue());
				} 
			}
			yield return 0;
		}
		Debug.LogError ("Error: "+socketError);
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
		w.Close ();
	}

	public IEnumerator ConnectWebsocket()
	{
		//maybe need to add some code here to check protocol of URI to make sure
		// it is "ws" or "wss". Not sure what happens when you throw an exception
		// in a monobehavior function.
		w = new WebSocketSharp.WebSocket(address.ToString());
		w.OnMessage += gotMessage;
		w.OnOpen += (sender, e) => socketIsConnected = true;
		w.OnError += (sender, e) => socketError = e.Message;
		w.ConnectAsync();
		while (!socketIsConnected && socketError == null)
			yield return 0;
	}

	public void gotMessage(object sender, WebSocketSharp.MessageEventArgs msg) {

		if (msg.Type == WebSocketSharp.Opcode.Binary) {
			lock (updateLock) {
				updateMsg.ClearVrmsg ();
				updateMsg.MergeFrom (new Google.Protobuf.CodedInputStream (msg.RawData));
				Google.Protobuf.VRCom.Update.VrmsgOneofCase msgType = updateMsg.VrmsgCase;
				switch (msgType) {
				case Google.Protobuf.VRCom.Update.VrmsgOneofCase.Mocap:
					mocapMsg = updateMsg.Clone();
					break;
				case Google.Protobuf.VRCom.Update.VrmsgOneofCase.Hydra:
					hydraMsg = updateMsg.Clone();
					break;
				case Google.Protobuf.VRCom.Update.VrmsgOneofCase.Wiimote:
					wiimoteMsgs.Enqueue (updateMsg.Clone());
					break;
				default:
					Debug.Log ("Received an unknown or empty message");
					break;
				}
			}
		} else {
			//jmsg = JsonUtility.FromJson (msg.Data);
			Debug.Log(msg.Data);
		}	
		
	}
}
